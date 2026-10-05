namespace NextRoleAI.Domain.AgentWorkflows;

public sealed class AgentWorkflowStep
{
    public Guid Id { get; set; }

    public Guid WorkflowRunId { get; set; }

    public int Sequence { get; set; }

    public string AgentName { get; set; } = string.Empty;

    public string Responsibility { get; set; } = string.Empty;

    public string AllowedTools { get; set; } = string.Empty;

    public AgentStepStatus Status { get; set; }

    public string InputJson { get; set; } = "{}";

    public string? OutputJson { get; set; }

    public int RetryCount { get; set; }

    public string? Error { get; set; }

    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public long? DurationMilliseconds { get; set; }

    public AgentWorkflowRun WorkflowRun { get; set; } = null!;

    public ICollection<AgentToolCall> ToolCalls { get; } = [];
}
