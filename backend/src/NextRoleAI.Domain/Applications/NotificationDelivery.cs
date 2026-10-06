namespace NextRoleAI.Domain.Applications;

public sealed class NotificationDelivery
{
    public Guid Id { get; set; }

    public Guid JobApplicationId { get; set; }

    public string RecipientUserId { get; set; } = string.Empty;

    public string RecipientEmail { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public NotificationDeliveryStatus Status { get; set; } = NotificationDeliveryStatus.Pending;

    public string? ProviderMessageId { get; set; }

    public string? FailureReason { get; set; }

    public int AttemptCount { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? AttemptedAtUtc { get; set; }

    public JobApplication JobApplication { get; set; } = null!;
}
