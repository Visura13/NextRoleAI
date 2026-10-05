namespace NextRoleAI.Domain.Cvs;

public sealed class CvDocument
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string Sha256Checksum { get; set; } = string.Empty;

    public CvProcessingStatus Status { get; set; }

    public string ExtractedText { get; set; } = string.Empty;

    public string CandidateName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string CurrentJobTitle { get; set; } = string.Empty;

    public string ProfessionalSummary { get; set; } = string.Empty;

    public int YearsExperience { get; set; }

    public string? FailureReason { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<CvSkill> Skills { get; } = [];
}
