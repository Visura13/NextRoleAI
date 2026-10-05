namespace NextRoleAI.Application.AgentWorkflows;

public interface IAgentWorkflowService
{
    Task<AgentWorkflowCommandResult> StartAsync(
        string userId,
        StartAgentWorkflow request,
        CancellationToken cancellationToken = default);

    Task<AgentWorkflowListResult> ListAsync(
        string userId,
        AgentWorkflowQuery query,
        CancellationToken cancellationToken = default);

    Task<AgentWorkflowResult?> GetAsync(
        string userId,
        Guid workflowId,
        CancellationToken cancellationToken = default);

    Task<AgentWorkflowCommandResult> DecideAsync(
        string userId,
        Guid workflowId,
        AgentWorkflowDecision decision,
        CancellationToken cancellationToken = default);
}
