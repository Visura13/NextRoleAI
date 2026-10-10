using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using NextRoleAI.Application.Cvs;
using NextRoleAI.Domain.Cvs;
using NextRoleAI.Infrastructure.Persistence;

namespace NextRoleAI.Infrastructure.Cvs;

internal sealed class CvService(
    ApplicationDbContext dbContext,
    ICvFileStore fileStore,
    ICvTextExtractor textExtractor,
    ICvProfileParser profileParser,
    TimeProvider timeProvider) : ICvService
{
    private const long MaximumFileBytes = 5 * 1024 * 1024;
    private const int MaximumExtractedTextLength = 50_000;

    private static readonly IReadOnlyDictionary<string, string> SupportedTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

    public async Task<CvResult?> GetAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cv = await dbContext.CvDocuments
            .AsNoTracking()
            .Include(document => document.Skills)
            .SingleOrDefaultAsync(document => document.UserId == userId, cancellationToken);
        return cv is null ? null : ToResult(cv);
    }

    public async Task<CvCommandResult> UploadAsync(
        string userId,
        CvUpload upload,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(upload.FileName).ToLowerInvariant();
        var validationError = ValidateMetadata(upload, extension);
        if (validationError is not null)
        {
            return CvCommandResult.Failure(CvCommandError.InvalidFile, validationError);
        }

        await using var buffer = new MemoryStream((int)upload.Length);
        await upload.Content.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length != upload.Length || !HasValidSignature(buffer, extension))
        {
            return CvCommandResult.Failure(
                CvCommandError.InvalidFile,
                "The file content does not match its PDF or DOCX extension.");
        }

        string extractedText;
        try
        {
            extractedText = await textExtractor.ExtractAsync(
                buffer,
                extension,
                cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return CvCommandResult.Failure(
                CvCommandError.ExtractionFailed,
                "The document could not be read. Export it as a standard PDF or DOCX and try again.");
        }

        extractedText = extractedText.Trim();
        if (extractedText.Length < 20)
        {
            return CvCommandResult.Failure(
                CvCommandError.ExtractionFailed,
                "No readable CV text was found. Scanned-image PDFs are not supported yet.");
        }

        if (extractedText.Length > MaximumExtractedTextLength)
        {
            extractedText = extractedText[..MaximumExtractedTextLength];
        }

        var parsed = profileParser.Parse(extractedText);
        var bytes = buffer.ToArray();
        var checksum = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var storageKey = $"{Guid.NewGuid():N}{extension}";
        buffer.Position = 0;
        await fileStore.StoreAsync(storageKey, buffer, cancellationToken);

        var previous = await dbContext.CvDocuments
            .Include(document => document.Skills)
            .SingleOrDefaultAsync(document => document.UserId == userId, cancellationToken);
        var now = timeProvider.GetUtcNow();
        var previousStorageKey = previous?.StorageKey;
        var cv = previous ?? new CvDocument
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAtUtc = now
        };

        if (previous is not null)
        {
            dbContext.CvSkills.RemoveRange(previous.Skills);
            previous.Skills.Clear();
        }

        cv.OriginalFileName = SafeDisplayName(upload.FileName);
        cv.StorageKey = storageKey;
        cv.ContentType = SupportedTypes[extension];
        cv.SizeBytes = buffer.Length;
        cv.Sha256Checksum = checksum;
        cv.Status = CvProcessingStatus.NeedsReview;
        cv.ExtractedText = extractedText;
        cv.CandidateName = LimitLength(parsed.CandidateName, 150);
        cv.Email = LimitLength(parsed.Email, 254);
        cv.Phone = LimitLength(parsed.Phone, 40);
        cv.Location = LimitLength(parsed.Location, 150);
        cv.CurrentJobTitle = LimitLength(parsed.CurrentJobTitle, 150);
        cv.ProfessionalSummary = LimitLength(parsed.ProfessionalSummary, 2000);
        cv.YearsExperience = parsed.YearsExperience;
        cv.FailureReason = null;
        cv.UpdatedAtUtc = now;
        AddSkills(cv, parsed.Skills);

        if (previous is null)
        {
            dbContext.CvDocuments.Add(cv);
        }
        else
        {
            dbContext.CvSkills.AddRange(cv.Skills);
        }
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await fileStore.DeleteAsync(storageKey, CancellationToken.None);
            throw;
        }

        if (previousStorageKey is not null)
        {
            await fileStore.DeleteAsync(previousStorageKey, CancellationToken.None);
        }

        return CvCommandResult.Success(ToResult(cv));
    }

    public async Task<CvCommandResult> ConfirmProfileAsync(
        string userId,
        CvProfileUpdate update,
        CancellationToken cancellationToken = default)
    {
        var cv = await dbContext.CvDocuments
            .Include(document => document.Skills)
            .SingleOrDefaultAsync(document => document.UserId == userId, cancellationToken);
        if (cv is null)
        {
            return CvCommandResult.Failure(
                CvCommandError.NotFound,
                "Upload a CV before confirming its profile.");
        }

        cv.CandidateName = update.CandidateName.Trim();
        cv.Email = update.Email.Trim();
        cv.Phone = update.Phone.Trim();
        cv.Location = update.Location.Trim();
        cv.CurrentJobTitle = update.CurrentJobTitle.Trim();
        cv.ProfessionalSummary = update.ProfessionalSummary.Trim();
        cv.YearsExperience = update.YearsExperience;
        cv.Status = CvProcessingStatus.Confirmed;
        cv.FailureReason = null;
        cv.UpdatedAtUtc = timeProvider.GetUtcNow();

        dbContext.CvSkills.RemoveRange(cv.Skills);
        cv.Skills.Clear();
        AddSkills(cv, update.Skills);
        dbContext.CvSkills.AddRange(cv.Skills);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CvCommandResult.Success(ToResult(cv));
    }

    public async Task<bool> DeleteAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cv = await dbContext.CvDocuments
            .SingleOrDefaultAsync(document => document.UserId == userId, cancellationToken);
        if (cv is null)
        {
            return false;
        }

        dbContext.CvDocuments.Remove(cv);
        await dbContext.SaveChangesAsync(cancellationToken);
        await fileStore.DeleteAsync(cv.StorageKey, CancellationToken.None);
        return true;
    }

    private string? ValidateMetadata(CvUpload upload, string extension)
    {
        if (upload.Length <= 0 || upload.Length > MaximumFileBytes)
        {
            return "The CV must be a non-empty file no larger than 5 MB.";
        }

        if (!SupportedTypes.TryGetValue(extension, out var expectedType) ||
            !textExtractor.Supports(extension))
        {
            return "Only PDF and DOCX CV files are supported.";
        }

        if (!upload.ContentType.Equals(expectedType, StringComparison.OrdinalIgnoreCase) &&
            !upload.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return "The CV content type does not match its file extension.";
        }

        return null;
    }

    private static bool HasValidSignature(Stream stream, string extension)
    {
        stream.Position = 0;
        Span<byte> header = stackalloc byte[5];
        var read = stream.Read(header);
        stream.Position = 0;
        return extension switch
        {
            ".pdf" => read >= 5 && header.SequenceEqual("%PDF-"u8),
            ".docx" => read >= 4 &&
                header[..4].SequenceEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }),
            _ => false
        };
    }

    private static void AddSkills(CvDocument cv, IEnumerable<string> skills)
    {
        foreach (var skill in skills
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(skill => skill.Trim())
            .Where(skill => skill.Length <= 100)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(skill => skill, StringComparer.OrdinalIgnoreCase)
            .Take(50))
        {
            cv.Skills.Add(new CvSkill
            {
                Id = Guid.NewGuid(),
                CvDocumentId = cv.Id,
                Name = skill,
                CvDocument = cv
            });
        }
    }

    private static string SafeDisplayName(string fileName)
    {
        var safe = Path.GetFileName(fileName).Trim();
        if (safe.Length == 0)
        {
            return "cv";
        }

        return safe.Length <= 255 ? safe : safe[..255];
    }

    private static string LimitLength(string value, int maximumLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maximumLength
            ? trimmed
            : trimmed[..maximumLength].TrimEnd();
    }

    private static CvResult ToResult(CvDocument cv) =>
        new(
            cv.Id,
            cv.OriginalFileName,
            cv.ContentType,
            cv.SizeBytes,
            cv.Sha256Checksum,
            cv.Status,
            cv.CandidateName,
            cv.Email,
            cv.Phone,
            cv.Location,
            cv.CurrentJobTitle,
            cv.ProfessionalSummary,
            cv.YearsExperience,
            cv.Skills
                .OrderBy(skill => skill.Name, StringComparer.OrdinalIgnoreCase)
                .Select(skill => skill.Name)
                .ToArray(),
            cv.FailureReason,
            cv.CreatedAtUtc,
            cv.UpdatedAtUtc);
}
