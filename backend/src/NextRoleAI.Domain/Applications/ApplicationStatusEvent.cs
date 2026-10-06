namespace NextRoleAI.Domain.Applications;

public sealed class ApplicationStatusEvent
{
    public Guid Id { get; set; }

    public Guid JobApplicationId { get; set; }

    public ApplicationStatus? PreviousStatus { get; set; }

    public ApplicationStatus NewStatus { get; set; }

    public string ActorUserId { get; set; } = string.Empty;

    public ApplicationActorRole ActorRole { get; set; }

    public string Note { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public JobApplication JobApplication { get; set; } = null!;
}
