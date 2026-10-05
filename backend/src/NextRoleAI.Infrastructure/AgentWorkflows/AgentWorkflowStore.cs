using Microsoft.EntityFrameworkCore;
using NextRoleAI.Application.AgentWorkflows;
using NextRoleAI.Domain.AgentWorkflows;
using NextRoleAI.Infrastructure.Persistence;

namespace NextRoleAI.Infrastructure.AgentWorkflows;

internal sealed class AgentWorkflowStore(ApplicationDbContext dbContext)
    : IAgentWorkflowStore
{
    public async Task AddAsync(
        AgentWorkflowRun workflow,
        CancellationToken cancellationToken = default) =>
        await dbContext.AgentWorkflowRuns.AddAsync(workflow, cancellationToken);

    public void AddStep(AgentWorkflowStep step) =>
        dbContext.AgentWorkflowSteps.Add(step);

    public void AddToolCall(AgentToolCall toolCall) =>
        dbContext.AgentToolCalls.Add(toolCall);

    public void AddValidationResult(AgentValidationResult validationResult) =>
        dbContext.AgentValidationResults.Add(validationResult);

    public void AddShortlistItem(AgentShortlistItem shortlistItem) =>
        dbContext.AgentShortlistItems.Add(shortlistItem);

    public void AddApprovalDecision(AgentApprovalDecision approvalDecision) =>
        dbContext.AgentApprovalDecisions.Add(approvalDecision);

    public Task<AgentWorkflowRun?> GetOwnedAsync(
        string userId,
        Guid workflowId,
        CancellationToken cancellationToken = default) =>
        Query()
            .SingleOrDefaultAsync(
                workflow => workflow.Id == workflowId && workflow.UserId == userId,
                cancellationToken);

    public async Task<(IReadOnlyCollection<AgentWorkflowRun> Items, int TotalCount)> ListOwnedAsync(
        string userId,
        AgentWorkflowQuery query,
        CancellationToken cancellationToken = default)
    {
        var source = dbContext.AgentWorkflowRuns
            .Where(workflow => workflow.UserId == userId);
        if (query.Status.HasValue)
        {
            source = source.Where(workflow => workflow.Status == query.Status.Value);
        }

        var totalCount = await source.CountAsync(cancellationToken);
        var ids = await source
            .OrderByDescending(workflow => workflow.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(workflow => workflow.Id)
            .ToArrayAsync(cancellationToken);
        var workflows = await Query()
            .Where(workflow => ids.Contains(workflow.Id))
            .ToArrayAsync(cancellationToken);
        var ordered = ids
            .Join(workflows, id => id, workflow => workflow.Id, (_, workflow) => workflow)
            .ToArray();
        return (ordered, totalCount);
    }

    public Task SaveAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<AgentWorkflowRun> Query() =>
        dbContext.AgentWorkflowRuns
            .AsSplitQuery()
            .Include(workflow => workflow.Steps)
                .ThenInclude(step => step.ToolCalls)
            .Include(workflow => workflow.ValidationResults)
            .Include(workflow => workflow.ShortlistItems)
                .ThenInclude(item => item.JobPosting)
                    .ThenInclude(job => job.CompanyProfile)
            .Include(workflow => workflow.ApprovalDecisions);
}
