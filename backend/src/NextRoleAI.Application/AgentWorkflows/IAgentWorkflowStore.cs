using NextRoleAI.Domain.AgentWorkflows;

namespace NextRoleAI.Application.AgentWorkflows;

public interface IAgentWorkflowStore
{
    Task AddAsync(
        AgentWorkflowRun workflow,
        CancellationToken cancellationToken = default);

    void AddStep(AgentWorkflowStep step);

    void AddToolCall(AgentToolCall toolCall);

    void AddValidationResult(AgentValidationResult validationResult);

    void AddShortlistItem(AgentShortlistItem shortlistItem);

    void AddApprovalDecision(AgentApprovalDecision approvalDecision);

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
