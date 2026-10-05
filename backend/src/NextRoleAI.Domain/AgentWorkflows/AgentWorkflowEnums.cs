namespace NextRoleAI.Domain.AgentWorkflows;

public enum AgentWorkflowStatus
{
    Running,
    PendingApproval,
    Completed,
    Rejected,
    RevisionRequested,
    Failed
}

public enum AgentApprovalStatus
{
    NotRequested,
    Pending,
    Approved,
    Rejected,
    RevisionRequested
}

public enum AgentStepStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    AwaitingApproval
}

public enum AgentDecisionType
{
    Approve,
    Reject,
    RequestRevision
}
