using NextRoleAI.Domain.AgentWorkflows;

namespace NextRoleAI.Application.AgentWorkflows;

public interface IAgentWorkflowStore
{
    Task AddAsync(
        AgentWorkflowRun workflow,
        CancellationToken cancellationToken = default);

    Task<AgentWorkflowRun?> GetOwnedAsync(
        string userId,
        Guid workflowId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyCollection<AgentWorkflowRun> Items, int TotalCount)> ListOwnedAsync(
        string userId,
        AgentWorkflowQuery query,
        CancellationToken cancellationToken = default);

    Task SaveAsync(CancellationToken cancellationToken = default);
}
