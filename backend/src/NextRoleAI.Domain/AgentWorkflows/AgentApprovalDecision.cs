namespace NextRoleAI.Domain.AgentWorkflows;

public sealed class AgentApprovalDecision
{
    public Guid Id { get; set; }

    public Guid WorkflowRunId { get; set; }

    public string DecidedByUserId { get; set; } = string.Empty;

    public AgentDecisionType Decision { get; set; }

    public string Feedback { get; set; } = string.Empty;

    public DateTimeOffset DecidedAtUtc { get; set; }

    public AgentWorkflowRun WorkflowRun { get; set; } = null!;
}
