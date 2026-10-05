using NextRoleAI.Application.AgentWorkflows;

namespace NextRoleAI.AgenticAI;

public sealed record PlannedAgentStep(
    string AgentName,
    string Responsibility,
    IReadOnlyCollection<string> AllowedTools);

public sealed record WorkflowPlanOutput(
    string ObjectiveSummary,
    int DesiredCount,
    bool RemoteOnly,
    IReadOnlyCollection<PlannedAgentStep> Steps);

public sealed record ValidationRuleOutput(
    string RuleName,
    bool Passed,
    string Message);

internal sealed class AgentExecutionException(
    string code,
    string message,
    bool isTransient = false,
    Exception? innerException = null) : Exception(message, innerException)
{
    public string Code { get; } = code;

    public bool IsTransient { get; } = isTransient;
}

internal static class AgentDefinitions
{
    public const string Planning = "Planning Agent";
    public const string CandidateProfile = "Candidate Profile Agent";
    public const string JobDiscovery = "Job Discovery Agent";
    public const string ValidationSafety = "Validation & Safety Agent";
    public const string HumanApproval = "Human Approval Gate";

    public static readonly PlannedAgentStep PlanningStep = new(
        Planning,
        "Convert the bounded job-search objective into a typed plan and delegate each step.",
        []);

    public static readonly PlannedAgentStep CandidateProfileStep = new(
        CandidateProfile,
        "Read the confirmed candidate profile through the least-privilege profile tool.",
        [AgentToolNames.ReadCandidateProfile]);

    public static readonly PlannedAgentStep JobDiscoveryStep = new(
        JobDiscovery,
        "Discover and rank published roles, then prepare a bounded shortlist.",
        [AgentToolNames.RankPublishedJobs]);

    public static readonly PlannedAgentStep ValidationStep = new(
        ValidationSafety,
        "Apply schema, business-rule, permission, score, and prompt-safety checks.",
        []);

    public static readonly PlannedAgentStep PublicationStep = new(
        JobDiscovery,
        "Publish only the human-approved shortlist to the Job Seeker account.",
        [AgentToolNames.PublishShortlist]);
}
