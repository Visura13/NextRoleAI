namespace NextRoleAI.Infrastructure.Cvs;

public sealed class CvStorageOptions
{
    public const string SectionName = "CvStorage";

    public string RootPath { get; init; } = string.Empty;
}
