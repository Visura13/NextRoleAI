using NextRoleAI.Domain.AgentWorkflows;
using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Application.AgentWorkflows;

public sealed record StartAgentWorkflow(string Objective);

public sealed record AgentWorkflowDecision(
    AgentDecisionType Decision,
    string Feedback,
    string? RevisedObjective);

public sealed record AgentWorkflowStepResult(
    Guid Id,
    int Sequence,
    string AgentName,
    string Responsibility,
    IReadOnlyCollection<string> AllowedTools,
    AgentStepStatus Status,
    int RetryCount,
    string? Error,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    long? DurationMilliseconds,
    IReadOnlyCollection<AgentToolCallResult> ToolCalls);

public sealed record AgentToolCallResult(
    Guid Id,
    string ToolName,
    bool Succeeded,
    string? Error,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    long DurationMilliseconds);

public sealed record AgentValidationResultModel(
    string RuleName,
    bool Passed,
    string Message,
    DateTimeOffset CreatedAtUtc);

public sealed record AgentShortlistResult(
    Guid JobId,
    string CompanyName,
    string Title,
    string Location,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    decimal Score,
    int Rank,
    string ReasonSummary,
    bool IsApproved);

public sealed record AgentApprovalDecisionResult(
    AgentDecisionType Decision,
    string Feedback,
    DateTimeOffset DecidedAtUtc);

public sealed record AgentWorkflowResult(
    Guid Id,
    string Objective,
    AgentWorkflowStatus Status,
    AgentApprovalStatus ApprovalStatus,
    string CurrentAgent,
    int RevisionNumber,
    string? FailureCode,
    string? FailureMessage,
    string? FinalSummary,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyCollection<AgentWorkflowStepResult> Steps,
    IReadOnlyCollection<AgentValidationResultModel> ValidationResults,
    IReadOnlyCollection<AgentShortlistResult> Shortlist,
    IReadOnlyCollection<AgentApprovalDecisionResult> ApprovalDecisions);

public sealed record AgentWorkflowListResult(
    IReadOnlyCollection<AgentWorkflowResult> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling((double)TotalCount / PageSize);
}

public enum AgentWorkflowCommandError
{
    None,
    NotFound,
    InvalidState,
    InvalidRevision
}

public sealed record AgentWorkflowCommandResult(
    AgentWorkflowResult? Workflow,
    AgentWorkflowCommandError Error = AgentWorkflowCommandError.None,
    string? Message = null)
{
    public bool Succeeded => Error == AgentWorkflowCommandError.None;

    public static AgentWorkflowCommandResult Success(AgentWorkflowResult workflow) =>
        new(workflow);

    public static AgentWorkflowCommandResult Failure(
        AgentWorkflowCommandError error,
        string message) => new(null, error, message);
}

public sealed record AgentWorkflowQuery(
    AgentWorkflowStatus? Status,
    int Page,
    int PageSize);

public sealed record AgentToolContext(
    string UserId,
    Guid WorkflowId,
    Guid StepId);

public sealed record AgentToolExecutionResult(
    bool Succeeded,
    string OutputJson,
    string? Error = null,
    bool IsTransient = false);

public sealed record CandidateProfileToolOutput(
    bool IsConfirmed,
    string CurrentJobTitle,
    string PreferredJobTitle,
    string Location,
    decimal? PreferredSalary,
    int YearsExperience,
    IReadOnlyCollection<string> Skills);

public sealed record RankedJobsToolInput(int Limit);

public sealed record RankedJobToolItem(
    Guid JobId,
    string CompanyName,
    string Title,
    string Location,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    decimal Score,
    IReadOnlyCollection<string> Reasons,
    IReadOnlyCollection<string> MissingRequiredSkills);

public sealed record RankedJobsToolOutput(
    IReadOnlyCollection<RankedJobToolItem> Items);

public sealed record PublishShortlistToolInput(
    IReadOnlyCollection<Guid> JobIds);

public sealed record PublishShortlistToolOutput(int PublishedCount);
