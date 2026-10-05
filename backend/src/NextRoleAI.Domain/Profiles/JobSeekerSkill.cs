namespace NextRoleAI.Domain.Profiles;

public sealed class JobSeekerSkill
{
    public Guid Id { get; set; }

    public Guid JobSeekerProfileId { get; set; }

    public string Name { get; set; } = string.Empty;

    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
}
