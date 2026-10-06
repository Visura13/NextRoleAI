using NextRoleAI.Domain.AgentWorkflows;
using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Domain.Applications;

public sealed class JobApplication
{
    public Guid Id { get; set; }

    public string JobSeekerUserId { get; set; } = string.Empty;

    public Guid JobPostingId { get; set; }

    public Guid? SourceWorkflowRunId { get; set; }

    public string CoverNote { get; set; } = string.Empty;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public JobPosting JobPosting { get; set; } = null!;

    public AgentWorkflowRun? SourceWorkflowRun { get; set; }

    public ICollection<ApplicationStatusEvent> StatusEvents { get; } = [];

    public ICollection<NotificationDelivery> NotificationDeliveries { get; } = [];
}
