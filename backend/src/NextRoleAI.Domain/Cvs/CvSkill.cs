namespace NextRoleAI.Domain.Cvs;

public sealed class CvSkill
{
    public Guid Id { get; set; }

    public Guid CvDocumentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public CvDocument CvDocument { get; set; } = null!;
}
