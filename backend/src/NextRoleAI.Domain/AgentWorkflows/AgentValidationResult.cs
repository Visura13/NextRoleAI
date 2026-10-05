namespace NextRoleAI.Domain.AgentWorkflows;

public sealed class AgentValidationResult
{
    public Guid Id { get; set; }

    public Guid WorkflowRunId { get; set; }

    public string RuleName { get; set; } = string.Empty;

    public bool Passed { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public AgentWorkflowRun WorkflowRun { get; set; } = null!;
}
