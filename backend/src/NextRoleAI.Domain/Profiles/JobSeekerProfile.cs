namespace NextRoleAI.Domain.Profiles;

public sealed class JobSeekerProfile
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Headline { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string PreferredJobTitle { get; set; } = string.Empty;

    public decimal? PreferredSalary { get; set; }

    public int YearsOfExperience { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<JobSeekerSkill> Skills { get; } = [];
}
