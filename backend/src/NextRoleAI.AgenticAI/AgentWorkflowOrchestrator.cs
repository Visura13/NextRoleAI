using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using NextRoleAI.Application.AgentWorkflows;
using NextRoleAI.Domain.AgentWorkflows;

namespace NextRoleAI.AgenticAI;

public sealed class AgentWorkflowOrchestrator(
    IAgentWorkflowStore store,
    IEnumerable<IAgentTool> tools,
    PlanningAgent planningAgent,
    CandidateProfileAgent candidateProfileAgent,
    JobDiscoveryAgent jobDiscoveryAgent,
    ValidationSafetyAgent validationSafetyAgent,
    TimeProvider timeProvider) : IAgentWorkflowService
{
    private const int MaximumRetries = 1;
    private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(10);
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly IReadOnlyCollection<IAgentTool> agentTools = tools.ToArray();

    public async Task<AgentWorkflowCommandResult> StartAsync(
        string userId,
        StartAgentWorkflow request,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var workflow = new AgentWorkflowRun
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Objective = request.Objective.Trim(),
            Status = AgentWorkflowStatus.Running,
            ApprovalStatus = AgentApprovalStatus.NotRequested,
            CurrentAgent = AgentDefinitions.Planning,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        await store.AddAsync(workflow, cancellationToken);
        await store.SaveAsync(cancellationToken);
        await ExecuteCycleAsync(workflow, cancellationToken);

        var hydrated = await store.GetOwnedAsync(userId, workflow.Id, cancellationToken)
            ?? workflow;
        return AgentWorkflowCommandResult.Success(ToResult(hydrated));
    }

    public async Task<AgentWorkflowListResult> ListAsync(
        string userId,
        AgentWorkflowQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await store.ListOwnedAsync(
            userId,
            query,
            cancellationToken);
        return new AgentWorkflowListResult(
            items.Select(ToResult).ToArray(),
            query.Page,
            query.PageSize,
            totalCount);
    }

    public async Task<AgentWorkflowResult?> GetAsync(
        string userId,
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        var workflow = await store.GetOwnedAsync(userId, workflowId, cancellationToken);
        return workflow is null ? null : ToResult(workflow);
    }

    public async Task<AgentWorkflowCommandResult> DecideAsync(
        string userId,
        Guid workflowId,
        AgentWorkflowDecision decision,
        CancellationToken cancellationToken = default)
    {
        var workflow = await store.GetOwnedAsync(userId, workflowId, cancellationToken);
        if (workflow is null)
        {
            return AgentWorkflowCommandResult.Failure(
                AgentWorkflowCommandError.NotFound,
                "The workflow was not found.");
        }

        if (workflow.Status != AgentWorkflowStatus.PendingApproval ||
            workflow.ApprovalStatus != AgentApprovalStatus.Pending)
        {
            return AgentWorkflowCommandResult.Failure(
                AgentWorkflowCommandError.InvalidState,
                "Only a workflow waiting for approval can receive a decision.");
        }

        if (decision.Decision == AgentDecisionType.RequestRevision &&
            string.IsNullOrWhiteSpace(decision.RevisedObjective))
        {
            return AgentWorkflowCommandResult.Failure(
                AgentWorkflowCommandError.InvalidRevision,
                "A revised objective is required when requesting revision.");
        }

        var now = timeProvider.GetUtcNow();
        workflow.ApprovalDecisions.Add(new AgentApprovalDecision
        {
            Id = Guid.NewGuid(),
            WorkflowRunId = workflow.Id,
            DecidedByUserId = userId,
            Decision = decision.Decision,
            Feedback = decision.Feedback.Trim(),
            DecidedAtUtc = now,
            WorkflowRun = workflow
        });
        CompleteApprovalGate(workflow, decision, now);

        switch (decision.Decision)
        {
            case AgentDecisionType.Reject:
                workflow.Status = AgentWorkflowStatus.Rejected;
                workflow.ApprovalStatus = AgentApprovalStatus.Rejected;
                workflow.CurrentAgent = AgentDefinitions.HumanApproval;
                workflow.FinalSummary = "The Job Seeker rejected the proposed shortlist. No jobs were published.";
                workflow.CompletedAtUtc = now;
                workflow.UpdatedAtUtc = now;
                await store.SaveAsync(cancellationToken);
                break;

            case AgentDecisionType.RequestRevision:
                workflow.Status = AgentWorkflowStatus.RevisionRequested;
                workflow.ApprovalStatus = AgentApprovalStatus.RevisionRequested;
                workflow.CurrentAgent = AgentDefinitions.HumanApproval;
                workflow.Objective = decision.RevisedObjective!.Trim();
                workflow.RevisionNumber++;
                workflow.FinalSummary = null;
                workflow.FailureCode = null;
                workflow.FailureMessage = null;
                workflow.CompletedAtUtc = null;
                workflow.ShortlistItems.Clear();
                workflow.UpdatedAtUtc = now;
                await store.SaveAsync(cancellationToken);
                workflow.Status = AgentWorkflowStatus.Running;
                workflow.ApprovalStatus = AgentApprovalStatus.NotRequested;
                await ExecuteCycleAsync(workflow, cancellationToken);
                break;

            case AgentDecisionType.Approve:
                workflow.ApprovalStatus = AgentApprovalStatus.Approved;
                workflow.Status = AgentWorkflowStatus.Running;
                workflow.UpdatedAtUtc = now;
                await store.SaveAsync(cancellationToken);
                await PublishApprovedShortlistAsync(workflow, cancellationToken);
                break;
        }

        var hydrated = await store.GetOwnedAsync(userId, workflow.Id, cancellationToken)
            ?? workflow;
        return AgentWorkflowCommandResult.Success(ToResult(hydrated));
    }

    private async Task ExecuteCycleAsync(
        AgentWorkflowRun workflow,
        CancellationToken cancellationToken)
    {
        try
        {
            var plan = await ExecuteStepAsync(
                workflow,
                AgentDefinitions.PlanningStep,
                new { workflow.Objective, workflow.RevisionNumber },
                (_, _) => Task.FromResult(planningAgent.Execute(workflow.Objective)),
                cancellationToken);

            var profile = await ExecuteStepAsync(
                workflow,
                AgentDefinitions.CandidateProfileStep,
                new { plan.ObjectiveSummary },
                async (step, token) =>
                {
                    var router = CreateToolRouter(workflow, step, AgentDefinitions.CandidateProfileStep);
                    var result = await candidateProfileAgent.ExecuteAsync(router, token);
                    if (!result.IsConfirmed || result.Skills.Count == 0)
                    {
                        throw new AgentExecutionException(
                            "ConfirmedCvRequired",
                            "Upload and confirm a CV profile before starting this workflow.");
                    }

                    return result;
                },
                cancellationToken);

            var shortlist = await ExecuteStepAsync(
                workflow,
                AgentDefinitions.JobDiscoveryStep,
                new
                {
                    plan.DesiredCount,
                    plan.RemoteOnly,
                    ProfileSkills = profile.Skills.Count
                },
                (step, token) => jobDiscoveryAgent.ExecuteAsync(
                    plan,
                    CreateToolRouter(workflow, step, AgentDefinitions.JobDiscoveryStep),
                    token),
                cancellationToken);

            var observedTools = workflow.Steps
                .Where(step => step.Sequence > PreviousCycleBoundary(workflow))
                .SelectMany(step => step.ToolCalls)
                .Select(call => call.ToolName)
                .ToArray();
            var validations = await ExecuteStepAsync(
                workflow,
                AgentDefinitions.ValidationStep,
                new
                {
                    PlannedSteps = plan.Steps.Count,
                    CandidateCount = shortlist.Count,
                    ObservedTools = observedTools
                },
                (_, _) => Task.FromResult(validationSafetyAgent.Execute(
                    workflow.Objective,
                    plan,
                    profile,
                    shortlist,
                    observedTools)),
                cancellationToken);

            foreach (var validation in validations)
            {
                workflow.ValidationResults.Add(new AgentValidationResult
                {
                    Id = Guid.NewGuid(),
                    WorkflowRunId = workflow.Id,
                    RuleName = $"revision-{workflow.RevisionNumber}:{validation.RuleName}",
                    Passed = validation.Passed,
                    Message = validation.Message,
                    CreatedAtUtc = timeProvider.GetUtcNow(),
                    WorkflowRun = workflow
                });
            }

            if (validations.Any(result => !result.Passed))
            {
                throw new AgentExecutionException(
                    "ValidationFailed",
                    "The proposal failed one or more deterministic safety checks.");
            }

            foreach (var (item, index) in shortlist.Select((item, index) => (item, index)))
            {
                workflow.ShortlistItems.Add(new AgentShortlistItem
                {
                    Id = Guid.NewGuid(),
                    WorkflowRunId = workflow.Id,
                    JobPostingId = item.JobId,
                    Rank = index + 1,
                    Score = item.Score,
                    ReasonSummary = string.Join(" ", item.Reasons.Take(3)),
                    IsApproved = false,
                    WorkflowRun = workflow
                });
            }

            AddApprovalGate(workflow);
            workflow.Status = AgentWorkflowStatus.PendingApproval;
            workflow.ApprovalStatus = AgentApprovalStatus.Pending;
            workflow.CurrentAgent = AgentDefinitions.HumanApproval;
            workflow.FinalSummary =
                $"{shortlist.Count} job(s) passed deterministic validation and await Job Seeker approval.";
            workflow.UpdatedAtUtc = timeProvider.GetUtcNow();
            await store.SaveAsync(cancellationToken);
        }
        catch (AgentExecutionException error)
        {
            await FailSafelyAsync(workflow, error.Code, error.Message, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            await FailSafelyAsync(
                workflow,
                "WorkflowTimeout",
                "The workflow exceeded its execution time limit and failed safely.",
                CancellationToken.None);
        }
    }

    private async Task PublishApprovedShortlistAsync(
        AgentWorkflowRun workflow,
        CancellationToken cancellationToken)
    {
        try
        {
            var jobIds = workflow.ShortlistItems
                .OrderBy(item => item.Rank)
                .Select(item => item.JobPostingId)
                .ToArray();
            var published = await ExecuteStepAsync(
                workflow,
                AgentDefinitions.PublicationStep,
                new { JobIds = jobIds },
                (step, token) => jobDiscoveryAgent.PublishAsync(
                    jobIds,
                    CreateToolRouter(workflow, step, AgentDefinitions.PublicationStep),
                    token),
                cancellationToken);

            workflow.Status = AgentWorkflowStatus.Completed;
            workflow.CurrentAgent = AgentDefinitions.JobDiscovery;
            workflow.FinalSummary =
                $"Published {published.PublishedCount} human-approved job recommendation(s).";
            workflow.CompletedAtUtc = timeProvider.GetUtcNow();
            workflow.UpdatedAtUtc = workflow.CompletedAtUtc.Value;
            await store.SaveAsync(cancellationToken);
        }
        catch (AgentExecutionException error)
        {
            await FailSafelyAsync(workflow, error.Code, error.Message, cancellationToken);
        }
    }

    private async Task<T> ExecuteStepAsync<T>(
        AgentWorkflowRun workflow,
        PlannedAgentStep definition,
        object input,
        Func<AgentWorkflowStep, CancellationToken, Task<T>> execute,
        CancellationToken cancellationToken)
    {
        var step = new AgentWorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowRunId = workflow.Id,
            Sequence = workflow.Steps.Count + 1,
            AgentName = definition.AgentName,
            Responsibility = definition.Responsibility,
            AllowedTools = string.Join(',', definition.AllowedTools),
            Status = AgentStepStatus.Pending,
            InputJson = JsonSerializer.Serialize(input, JsonOptions),
            WorkflowRun = workflow
        };
        workflow.Steps.Add(step);
        workflow.CurrentAgent = definition.AgentName;
        workflow.UpdatedAtUtc = timeProvider.GetUtcNow();
        await store.SaveAsync(cancellationToken);

        for (var attempt = 0; attempt <= MaximumRetries; attempt++)
        {
            step.Status = AgentStepStatus.Running;
            step.StartedAtUtc = timeProvider.GetUtcNow();
            step.Error = null;
            var stopwatch = Stopwatch.StartNew();
            await store.SaveAsync(cancellationToken);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(StepTimeout);
            try
            {
                var output = await execute(step, timeout.Token);
                stopwatch.Stop();
                step.OutputJson = JsonSerializer.Serialize(output, JsonOptions);
                step.Status = AgentStepStatus.Completed;
                step.CompletedAtUtc = timeProvider.GetUtcNow();
                step.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
                workflow.UpdatedAtUtc = step.CompletedAtUtc.Value;
                await store.SaveAsync(cancellationToken);
                return output;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var timeoutError = new AgentExecutionException(
                    "StepTimeout",
                    $"{definition.AgentName} exceeded the {StepTimeout.TotalSeconds:0}-second limit.",
                    true);
                if (attempt < MaximumRetries)
                {
                    step.RetryCount++;
                    step.Error = timeoutError.Message;
                    await store.SaveAsync(cancellationToken);
                    continue;
                }

                FinishFailedStep(step, stopwatch, timeoutError.Message);
                await store.SaveAsync(cancellationToken);
                throw timeoutError;
            }
            catch (AgentExecutionException error) when (error.IsTransient && attempt < MaximumRetries)
            {
                step.RetryCount++;
                step.Error = error.Message;
                await store.SaveAsync(cancellationToken);
            }
            catch (AgentExecutionException error)
            {
                FinishFailedStep(step, stopwatch, error.Message);
                await store.SaveAsync(cancellationToken);
                throw;
            }
            catch (Exception error)
            {
                FinishFailedStep(step, stopwatch, "The agent failed safely.");
                await store.SaveAsync(cancellationToken);
                throw new AgentExecutionException(
                    "AgentFailure",
                    $"{definition.AgentName} failed safely.",
                    false,
                    error);
            }
        }

        throw new UnreachableException();
    }

    private AgentToolRouter CreateToolRouter(
        AgentWorkflowRun workflow,
        AgentWorkflowStep step,
        PlannedAgentStep definition) =>
        new(agentTools, store, workflow, step, definition.AllowedTools, timeProvider);

    private void AddApprovalGate(AgentWorkflowRun workflow)
    {
        workflow.Steps.Add(new AgentWorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowRunId = workflow.Id,
            Sequence = workflow.Steps.Count + 1,
            AgentName = AgentDefinitions.HumanApproval,
            Responsibility = "Pause publication until the workflow owner approves, rejects, or revises it.",
            AllowedTools = string.Empty,
            Status = AgentStepStatus.AwaitingApproval,
            InputJson = JsonSerializer.Serialize(
                new { Action = AgentToolNames.PublishShortlist },
                JsonOptions),
            WorkflowRun = workflow
        });
    }

    private static void CompleteApprovalGate(
        AgentWorkflowRun workflow,
        AgentWorkflowDecision decision,
        DateTimeOffset completedAt)
    {
        var gate = workflow.Steps
            .Where(step => step.AgentName == AgentDefinitions.HumanApproval &&
                step.Status == AgentStepStatus.AwaitingApproval)
            .OrderByDescending(step => step.Sequence)
            .First();
        gate.Status = AgentStepStatus.Completed;
        gate.OutputJson = JsonSerializer.Serialize(
            new { decision.Decision, decision.Feedback },
            JsonOptions);
        gate.CompletedAtUtc = completedAt;
        gate.DurationMilliseconds = gate.StartedAtUtc.HasValue
            ? (long)(completedAt - gate.StartedAtUtc.Value).TotalMilliseconds
            : 0;
    }

    private async Task FailSafelyAsync(
        AgentWorkflowRun workflow,
        string code,
        string message,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        if (code is "UnsafeObjective" or "InvalidObjective")
        {
            workflow.ValidationResults.Add(new AgentValidationResult
            {
                Id = Guid.NewGuid(),
                WorkflowRunId = workflow.Id,
                RuleName = $"revision-{workflow.RevisionNumber}:PromptSafety",
                Passed = false,
                Message = message,
                CreatedAtUtc = now,
                WorkflowRun = workflow
            });
        }

        workflow.Status = AgentWorkflowStatus.Failed;
        workflow.ApprovalStatus = AgentApprovalStatus.NotRequested;
        workflow.CurrentAgent = "Safe failure";
        workflow.FailureCode = code;
        workflow.FailureMessage = message;
        workflow.FinalSummary = "The workflow stopped without publishing a shortlist.";
        workflow.UpdatedAtUtc = now;
        workflow.CompletedAtUtc = now;
        await store.SaveAsync(cancellationToken);
    }

    private void FinishFailedStep(
        AgentWorkflowStep step,
        Stopwatch stopwatch,
        string error)
    {
        stopwatch.Stop();
        step.Status = AgentStepStatus.Failed;
        step.Error = error;
        step.CompletedAtUtc = timeProvider.GetUtcNow();
        step.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
    }

    private static int PreviousCycleBoundary(AgentWorkflowRun workflow) =>
        workflow.Steps
            .Where(step => step.AgentName == AgentDefinitions.HumanApproval)
            .Select(step => step.Sequence)
            .DefaultIfEmpty(0)
            .Max();

    private static AgentWorkflowResult ToResult(AgentWorkflowRun workflow) =>
        new(
            workflow.Id,
            workflow.Objective,
            workflow.Status,
            workflow.ApprovalStatus,
            workflow.CurrentAgent,
            workflow.RevisionNumber,
            workflow.FailureCode,
            workflow.FailureMessage,
            workflow.FinalSummary,
            workflow.CreatedAtUtc,
            workflow.UpdatedAtUtc,
            workflow.CompletedAtUtc,
            workflow.Steps
                .OrderBy(step => step.Sequence)
                .Select(step => new AgentWorkflowStepResult(
                    step.Id,
                    step.Sequence,
                    step.AgentName,
                    step.Responsibility,
                    step.AllowedTools.Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                    step.Status,
                    step.RetryCount,
                    step.Error,
                    step.StartedAtUtc,
                    step.CompletedAtUtc,
                    step.DurationMilliseconds,
                    step.ToolCalls
                        .OrderBy(call => call.StartedAtUtc)
                        .Select(call => new AgentToolCallResult(
                            call.Id,
                            call.ToolName,
                            call.Succeeded,
                            call.Error,
                            call.StartedAtUtc,
                            call.CompletedAtUtc,
                            call.DurationMilliseconds))
                        .ToArray()))
                .ToArray(),
            workflow.ValidationResults
                .OrderBy(result => result.CreatedAtUtc)
                .Select(result => new AgentValidationResultModel(
                    result.RuleName,
                    result.Passed,
                    result.Message,
                    result.CreatedAtUtc))
                .ToArray(),
            workflow.ShortlistItems
                .OrderBy(item => item.Rank)
                .Select(item => new AgentShortlistResult(
                    item.JobPostingId,
                    item.JobPosting.CompanyProfile.Name,
                    item.JobPosting.Title,
                    item.JobPosting.Location,
                    item.JobPosting.EmploymentType,
                    item.JobPosting.WorkMode,
                    item.Score,
                    item.Rank,
                    item.ReasonSummary,
                    item.IsApproved))
                .ToArray(),
            workflow.ApprovalDecisions
                .OrderBy(decision => decision.DecidedAtUtc)
                .Select(decision => new AgentApprovalDecisionResult(
                    decision.Decision,
                    decision.Feedback,
                    decision.DecidedAtUtc))
                .ToArray());

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
