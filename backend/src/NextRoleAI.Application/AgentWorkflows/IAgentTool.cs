namespace NextRoleAI.Application.AgentWorkflows;

public interface IAgentTool
{
    string Name { get; }

    Task<AgentToolExecutionResult> ExecuteAsync(
        AgentToolContext context,
        string inputJson,
        CancellationToken cancellationToken = default);
}

public static class AgentToolNames
{
    public const string ReadCandidateProfile = "candidate-profile.read";
    public const string RankPublishedJobs = "published-jobs.rank";
    public const string PublishShortlist = "shortlist.publish";
}
