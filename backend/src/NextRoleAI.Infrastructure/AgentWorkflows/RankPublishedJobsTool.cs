using System.Text.Json;
using System.Text.Json.Serialization;
using NextRoleAI.Application.AgentWorkflows;
using NextRoleAI.Application.Recommendations;

namespace NextRoleAI.Infrastructure.AgentWorkflows;

internal sealed class RankPublishedJobsTool(IRecommendationService recommendationService)
    : IAgentTool
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public string Name => AgentToolNames.RankPublishedJobs;

    public async Task<AgentToolExecutionResult> ExecuteAsync(
        AgentToolContext context,
        string inputJson,
        CancellationToken cancellationToken = default)
    {
        RankedJobsToolInput? input;
        try
        {
            input = JsonSerializer.Deserialize<RankedJobsToolInput>(inputJson, JsonOptions);
        }
        catch (JsonException)
        {
            return new(false, "{}", "The ranking tool input did not match its schema.");
        }

        if (input is null || input.Limit is < 1 or > 100)
        {
            return new(false, "{}", "The ranking limit must be between 1 and 100.");
        }

        var result = await recommendationService.GetAsync(
            context.UserId,
            input.Limit,
            cancellationToken);
        if (!result.Succeeded)
        {
            return new(false, "{}", result.Message ?? "Recommendations are unavailable.");
        }

        var output = new RankedJobsToolOutput(result.Items
            .Select(item => new RankedJobToolItem(
                item.Job.Id,
                item.Job.CompanyName,
                item.Job.Title,
                item.Job.Location,
                item.Job.EmploymentType,
                item.Job.WorkMode,
                item.Score,
                item.Reasons,
                item.MissingRequiredSkills))
            .ToArray());
        return new AgentToolExecutionResult(
            true,
            JsonSerializer.Serialize(output, JsonOptions));
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
