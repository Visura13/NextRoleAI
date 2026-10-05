using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NextRoleAI.Application.AgentWorkflows;
using NextRoleAI.Infrastructure.Persistence;

namespace NextRoleAI.Infrastructure.AgentWorkflows;

internal sealed class PublishShortlistTool(ApplicationDbContext dbContext) : IAgentTool
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string Name => AgentToolNames.PublishShortlist;

    public async Task<AgentToolExecutionResult> ExecuteAsync(
        AgentToolContext context,
        string inputJson,
        CancellationToken cancellationToken = default)
    {
        PublishShortlistToolInput? input;
        try
        {
            input = JsonSerializer.Deserialize<PublishShortlistToolInput>(inputJson, JsonOptions);
        }
        catch (JsonException)
        {
            return new(false, "{}", "The shortlist publication input did not match its schema.");
        }

        var jobIds = input?.JobIds.Distinct().ToArray() ?? [];
        if (jobIds.Length is < 1 or > 5)
        {
            return new(false, "{}", "Between one and five unique jobs may be published.");
        }

        var items = await dbContext.AgentShortlistItems
            .Include(item => item.WorkflowRun)
            .Where(item => item.WorkflowRunId == context.WorkflowId &&
                item.WorkflowRun.UserId == context.UserId &&
                jobIds.Contains(item.JobPostingId))
            .ToArrayAsync(cancellationToken);
        if (items.Length != jobIds.Length)
        {
            return new(false, "{}", "The publication request contained a job outside the validated proposal.");
        }

        foreach (var item in items)
        {
            item.IsApproved = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new AgentToolExecutionResult(
            true,
            JsonSerializer.Serialize(
                new PublishShortlistToolOutput(items.Length),
                JsonOptions));
    }
}
