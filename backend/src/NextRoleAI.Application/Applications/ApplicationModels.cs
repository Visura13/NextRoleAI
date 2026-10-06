using NextRoleAI.Domain.Applications;

namespace NextRoleAI.Application.Applications;

public sealed record SubmitApplicationInput(
    Guid JobPostingId,
    string CoverNote,
    Guid? SourceWorkflowRunId);

public sealed record ApplicationStatusEventResult(
    Guid Id,
    ApplicationStatus? PreviousStatus,
    ApplicationStatus NewStatus,
    ApplicationActorRole ActorRole,
    string Note,
    DateTimeOffset CreatedAtUtc);

public sealed record NotificationDeliveryResult(
    string EventType,
    string Provider,
    NotificationDeliveryStatus Status,
    int AttemptCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? AttemptedAtUtc);

public sealed record JobApplicationResult(
    Guid Id,
    Guid JobPostingId,
    string JobTitle,
    string CompanyName,
    string JobLocation,
    string JobSeekerUserId,
    string JobSeekerName,
    string JobSeekerEmail,
    string CoverNote,
    ApplicationStatus Status,
    Guid? SourceWorkflowRunId,
    IReadOnlyCollection<ApplicationStatusEventResult> StatusHistory,
    IReadOnlyCollection<NotificationDeliveryResult> NotificationDeliveries,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record ApplicationListResult(
    IReadOnlyCollection<JobApplicationResult> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling((double)TotalCount / PageSize);
}

public enum ApplicationCommandError
{
    None,
    NotFound,
    JobUnavailable,
    DuplicateApplication,
    InvalidTransition,
    WorkflowNotApproved
}

public sealed record ApplicationCommandResult(
    JobApplicationResult? Application,
    ApplicationCommandError Error = ApplicationCommandError.None,
    string? Message = null)
{
    public bool Succeeded => Error == ApplicationCommandError.None;

    public static ApplicationCommandResult Success(JobApplicationResult application) =>
        new(application);

    public static ApplicationCommandResult Failure(
        ApplicationCommandError error,
        string message) => new(null, error, message);
}
