using NextRoleAI.Domain.Cvs;

namespace NextRoleAI.Application.Cvs;

public sealed record CvUpload(
    string FileName,
    string ContentType,
    long Length,
    Stream Content);

public sealed record CvProfileUpdate(
    string CandidateName,
    string Email,
    string Phone,
    string Location,
    string CurrentJobTitle,
    string ProfessionalSummary,
    int YearsExperience,
    IReadOnlyCollection<string> Skills,
    IReadOnlyCollection<CvEducationItem> Education);

public sealed record CvResult(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string Sha256Checksum,
    CvProcessingStatus Status,
    string CandidateName,
    string Email,
    string Phone,
    string Location,
    string CurrentJobTitle,
    string ProfessionalSummary,
    int YearsExperience,
    IReadOnlyCollection<string> Skills,
    IReadOnlyCollection<CvEducationItem> Education,
    CvQualityAssessment? QualityAssessment,
    string AnalysisMethod,
    string? AnalysisModel,
    string AnalysisPromptVersion,
    DateTimeOffset? AnalyzedAtUtc,
    string? FailureReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public enum CvCommandError
{
    None,
    NotFound,
    InvalidFile,
    ExtractionFailed
}

public sealed record CvCommandResult(
    CvResult? Cv,
    CvCommandError Error = CvCommandError.None,
    string? Message = null)
{
    public bool Succeeded => Error == CvCommandError.None;

    public static CvCommandResult Success(CvResult cv) => new(cv);

    public static CvCommandResult Failure(CvCommandError error, string message) =>
        new(null, error, message);
}

public sealed record ExtractedCvProfile(
    string CandidateName,
    string Email,
    string Phone,
    string Location,
    string CurrentJobTitle,
    string ProfessionalSummary,
    int YearsExperience,
    IReadOnlyCollection<string> Skills,
    IReadOnlyCollection<CvEducationItem>? Education = null);

public sealed record CvEducationItem(
    string Qualification,
    string FieldOfStudy,
    string Institution,
    string Status,
    string Evidence,
    decimal Confidence);

public sealed record CvQualityAssessment(
    int OverallScore,
    int CompletenessScore,
    int ClarityScore,
    int SkillsEvidenceScore,
    int ImpactScore,
    int AtsReadabilityScore,
    IReadOnlyCollection<string> Strengths,
    IReadOnlyCollection<string> Improvements);

public sealed record CvAnalysis(
    ExtractedCvProfile Profile,
    CvQualityAssessment? QualityAssessment,
    string AnalysisMethod,
    string? Model,
    string PromptVersion,
    DateTimeOffset AnalyzedAtUtc);
