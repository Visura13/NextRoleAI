namespace NextRoleAI.Domain.AgentWorkflows;

public sealed class AgentToolCall
{
    public Guid Id { get; set; }

    public Guid WorkflowStepId { get; set; }

    public string ToolName { get; set; } = string.Empty;

    public string InputJson { get; set; } = "{}";

    public string? OutputJson { get; set; }

    public bool Succeeded { get; set; }

    public string? Error { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset CompletedAtUtc { get; set; }

    public long DurationMilliseconds { get; set; }

    public AgentWorkflowStep WorkflowStep { get; set; } = null!;
}
