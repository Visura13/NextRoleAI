namespace NextRoleAI.Infrastructure.Applications;

public sealed class NotificationOptions
{
    public const string SectionName = "Notifications";

    public bool Enabled { get; init; }

    public string ResendApiKey { get; init; } = string.Empty;

    public string FromAddress { get; init; } = string.Empty;
}
