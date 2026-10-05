using System.Text.RegularExpressions;
using NextRoleAI.Application.AgentWorkflows;
using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.AgenticAI;

public sealed partial class PlanningAgent(ObjectiveGuard objectiveGuard)
{
    public WorkflowPlanOutput Execute(string objective)
    {
        var safeObjective = objectiveGuard.ValidateAndNormalise(objective);
        var countMatch = CountRegex().Match(safeObjective);
        var desiredCount = countMatch.Success &&
            int.TryParse(countMatch.Groups[1].Value, out var parsedCount)
                ? Math.Clamp(parsedCount, 1, 5)
                : 3;
        var remoteOnly = safeObjective.Contains("remote", StringComparison.OrdinalIgnoreCase) &&
            !safeObjective.Contains("hybrid", StringComparison.OrdinalIgnoreCase);

        return new WorkflowPlanOutput(
            safeObjective,
            desiredCount,
            remoteOnly,
            [
                AgentDefinitions.PlanningStep,
                AgentDefinitions.CandidateProfileStep,
                AgentDefinitions.JobDiscoveryStep,
                AgentDefinitions.ValidationStep
            ]);
    }

    [GeneratedRegex(@"\b([1-5])\b")]
    private static partial Regex CountRegex();
}

public sealed class CandidateProfileAgent
{
    public Task<CandidateProfileToolOutput> ExecuteAsync(
        AgentToolRouter tools,
        CancellationToken cancellationToken) =>
        tools.InvokeAsync<object, CandidateProfileToolOutput>(
            AgentToolNames.ReadCandidateProfile,
            new { },
            cancellationToken);
}

public sealed class JobDiscoveryAgent
{
    public async Task<IReadOnlyCollection<RankedJobToolItem>> ExecuteAsync(
        WorkflowPlanOutput plan,
        AgentToolRouter tools,
        CancellationToken cancellationToken)
    {
        var ranked = await tools.InvokeAsync<RankedJobsToolInput, RankedJobsToolOutput>(
            AgentToolNames.RankPublishedJobs,
            new RankedJobsToolInput(20),
            cancellationToken);
        var candidates = ranked.Items
            .Where(item => !plan.RemoteOnly || item.WorkMode == WorkMode.Remote)
            .Where(item => item.Score >= 40m)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .Take(plan.DesiredCount)
            .ToArray();

        if (candidates.Length == 0)
        {
            throw new AgentExecutionException(
                "NoSuitableJobs",
                "No published jobs satisfied the objective and the minimum safety score.");
        }

        return candidates;
    }

    public Task<PublishShortlistToolOutput> PublishAsync(
        IReadOnlyCollection<Guid> jobIds,
        AgentToolRouter tools,
        CancellationToken cancellationToken) =>
        tools.InvokeAsync<PublishShortlistToolInput, PublishShortlistToolOutput>(
            AgentToolNames.PublishShortlist,
            new PublishShortlistToolInput(jobIds),
            cancellationToken);
}

public sealed class ValidationSafetyAgent(ObjectiveGuard objectiveGuard)
{
    public IReadOnlyCollection<ValidationRuleOutput> Execute(
        string objective,
        WorkflowPlanOutput plan,
        CandidateProfileToolOutput profile,
        IReadOnlyCollection<RankedJobToolItem> shortlist,
        IReadOnlyCollection<string> observedTools)
    {
        var rules = new List<ValidationRuleOutput>();
        try
        {
            objectiveGuard.ValidateAndNormalise(objective);
            rules.Add(new("PromptSafety", true, "The objective passed instruction-injection checks."));
        }
        catch (AgentExecutionException error)
        {
            rules.Add(new("PromptSafety", false, error.Message));
        }

        rules.Add(new(
            "StructuredPlan",
            plan.Steps.Count == 4 && plan.Steps.Select(step => step.AgentName).Distinct().Count() == 4,
            "The plan delegates work to four distinct, typed agent roles."));
        rules.Add(new(
            "ConfirmedProfile",
            profile.IsConfirmed && profile.Skills.Count > 0,
            profile.IsConfirmed
                ? "A confirmed candidate profile with skills was supplied."
                : "A confirmed candidate profile is required."));
        rules.Add(new(
            "BoundedShortlist",
            shortlist.Count is > 0 and <= 5 && shortlist.Count <= plan.DesiredCount,
            $"The proposal contains {shortlist.Count} of at most {plan.DesiredCount} requested jobs."));
        rules.Add(new(
            "UniquePublishedJobs",
            shortlist.Select(item => item.JobId).Distinct().Count() == shortlist.Count,
            "Every proposed job identifier is unique and came from the published-job tool."));
        rules.Add(new(
            "ScoreRange",
            shortlist.All(item => item.Score is >= 40m and <= 100m),
            "Every proposed job has a deterministic score between 40 and 100."));

        var allowList = new HashSet<string>(
            [AgentToolNames.ReadCandidateProfile, AgentToolNames.RankPublishedJobs],
            StringComparer.Ordinal);
        rules.Add(new(
            "ToolAllowList",
            observedTools.All(allowList.Contains),
            "Every observed tool call was allow-listed for the pre-approval workflow."));
        return rules;
    }
}
