namespace NextRoleAI.Application.Applications;

public sealed record ApplicationNotification(
    string RecipientEmail,
    string Subject,
    string PlainTextBody);

public sealed record NotificationSendResult(
    bool Sent,
    bool Skipped,
    string Provider,
    string? ProviderMessageId,
    string? FailureReason)
{
    public static NotificationSendResult Delivered(string provider, string? providerMessageId) =>
        new(true, false, provider, providerMessageId, null);

    public static NotificationSendResult NotConfigured(string provider, string reason) =>
        new(false, true, provider, null, reason);

    public static NotificationSendResult Failed(string provider, string reason) =>
        new(false, false, provider, null, reason);
}

public interface IApplicationNotificationSender
{
    Task<NotificationSendResult> SendAsync(
        ApplicationNotification notification,
        CancellationToken cancellationToken = default);
}
