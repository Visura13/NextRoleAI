using NextRoleAI.Domain.Profiles;

namespace NextRoleAI.Domain.Jobs;

public sealed class JobPosting
{
    public Guid Id { get; set; }

    public Guid CompanyProfileId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public EmploymentType EmploymentType { get; set; }

    public WorkMode WorkMode { get; set; }

    public int MinimumYearsExperience { get; set; }

    public decimal? SalaryMinimum { get; set; }

    public decimal? SalaryMaximum { get; set; }

    public string? SalaryCurrency { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Draft;

    public DateTimeOffset? PublishedAtUtc { get; set; }

    public DateTimeOffset? ClosesAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public CompanyProfile CompanyProfile { get; set; } = null!;

    public ICollection<JobSkill> Skills { get; } = [];
}
