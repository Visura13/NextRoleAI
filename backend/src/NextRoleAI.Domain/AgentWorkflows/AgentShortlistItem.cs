using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Domain.AgentWorkflows;

public sealed class AgentShortlistItem
{
    public Guid Id { get; set; }

    public Guid WorkflowRunId { get; set; }

    public Guid JobPostingId { get; set; }

    public int Rank { get; set; }

    public decimal Score { get; set; }

    public string ReasonSummary { get; set; } = string.Empty;

    public bool IsApproved { get; set; }

    public AgentWorkflowRun WorkflowRun { get; set; } = null!;

    public JobPosting JobPosting { get; set; } = null!;
}
