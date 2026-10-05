namespace NextRoleAI.Domain.Jobs;

public sealed class JobSkill
{
    public Guid Id { get; set; }

    public Guid JobPostingId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public JobPosting JobPosting { get; set; } = null!;
}
