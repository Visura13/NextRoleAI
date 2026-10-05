using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Domain.Profiles;

public sealed class CompanyProfile
{
    public Guid Id { get; set; }

    public string RecruiterUserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string? WebsiteUrl { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<JobPosting> JobPostings { get; } = [];
}
