namespace NextRoleAI.Infrastructure.Cvs;

public sealed class CvAiOptions
{
    public const string SectionName = "CvAi";

    public bool Enabled { get; init; }

    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";

    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 45;

    public int MaximumInputCharacters { get; init; } = 20_000;
}
