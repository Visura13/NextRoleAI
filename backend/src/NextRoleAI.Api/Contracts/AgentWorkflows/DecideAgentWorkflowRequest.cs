using System.ComponentModel.DataAnnotations;
using NextRoleAI.Domain.AgentWorkflows;

namespace NextRoleAI.Api.Contracts.AgentWorkflows;

public sealed record DecideAgentWorkflowRequest(
    [Required] AgentDecisionType? Decision,
    [StringLength(1000)] string Feedback,
    [StringLength(500, MinimumLength = 20)] string? RevisedObjective);
