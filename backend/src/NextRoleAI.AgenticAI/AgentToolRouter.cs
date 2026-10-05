using System.Diagnostics;
using System.Text.Json;
using NextRoleAI.Application.AgentWorkflows;
using NextRoleAI.Domain.AgentWorkflows;

namespace NextRoleAI.AgenticAI;

public sealed class AgentToolRouter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IReadOnlyDictionary<string, IAgentTool> tools;
    private readonly IAgentWorkflowStore store;
    private readonly AgentWorkflowRun workflow;
    private readonly AgentWorkflowStep step;
    private readonly HashSet<string> allowedTools;
    private readonly TimeProvider timeProvider;

    public AgentToolRouter(
        IEnumerable<IAgentTool> tools,
        IAgentWorkflowStore store,
        AgentWorkflowRun workflow,
        AgentWorkflowStep step,
        IEnumerable<string> allowedTools,
        TimeProvider timeProvider)
    {
        this.tools = tools.ToDictionary(tool => tool.Name, StringComparer.Ordinal);
        this.store = store;
        this.workflow = workflow;
        this.step = step;
        this.allowedTools = allowedTools.ToHashSet(StringComparer.Ordinal);
        this.timeProvider = timeProvider;
    }

    public async Task<TOutput> InvokeAsync<TInput, TOutput>(
        string toolName,
        TInput input,
        CancellationToken cancellationToken)
    {
        if (!allowedTools.Contains(toolName) || !tools.TryGetValue(toolName, out var tool))
        {
            throw new AgentExecutionException(
                "ToolPermissionDenied",
                $"Agent '{step.AgentName}' is not permitted to call tool '{toolName}'.");
        }

        var startedAt = timeProvider.GetUtcNow();
        var stopwatch = Stopwatch.StartNew();
        var inputJson = JsonSerializer.Serialize(input, JsonOptions);
        AgentToolExecutionResult result;
        try
        {
            result = await tool.ExecuteAsync(
                new AgentToolContext(workflow.UserId, workflow.Id, step.Id),
                inputJson,
                cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            result = new AgentToolExecutionResult(
                false,
                "{}",
                "The tool failed safely.",
                true);
        }

        stopwatch.Stop();
        step.ToolCalls.Add(new AgentToolCall
        {
            Id = Guid.NewGuid(),
            WorkflowStepId = step.Id,
            ToolName = toolName,
            InputJson = inputJson,
            OutputJson = result.Succeeded ? result.OutputJson : null,
            Succeeded = result.Succeeded,
            Error = result.Error,
            StartedAtUtc = startedAt,
            CompletedAtUtc = timeProvider.GetUtcNow(),
            DurationMilliseconds = stopwatch.ElapsedMilliseconds,
            WorkflowStep = step
        });
        await store.SaveAsync(cancellationToken);

        if (!result.Succeeded)
        {
            throw new AgentExecutionException(
                "ToolFailure",
                result.Error ?? $"Tool '{toolName}' failed safely.",
                result.IsTransient);
        }

        try
        {
            return JsonSerializer.Deserialize<TOutput>(result.OutputJson, JsonOptions)
                ?? throw new JsonException("The tool returned an empty result.");
        }
        catch (JsonException error)
        {
            throw new AgentExecutionException(
                "InvalidToolOutput",
                $"Tool '{toolName}' returned an invalid structured result.",
                false,
                error);
        }
    }
}
