namespace NextRoleAI.Domain.AgentWorkflows;

public sealed class AgentWorkflowRun
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Objective { get; set; } = string.Empty;

    public AgentWorkflowStatus Status { get; set; }

    public AgentApprovalStatus ApprovalStatus { get; set; }

    public string CurrentAgent { get; set; } = string.Empty;

    public int RevisionNumber { get; set; }

    public string? FailureCode { get; set; }

    public string? FailureMessage { get; set; }

    public string? FinalSummary { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public ICollection<AgentWorkflowStep> Steps { get; } = [];

    public ICollection<AgentValidationResult> ValidationResults { get; } = [];

    public ICollection<AgentShortlistItem> ShortlistItems { get; } = [];

    public ICollection<AgentApprovalDecision> ApprovalDecisions { get; } = [];
}
