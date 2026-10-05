using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.AgentWorkflows;

public sealed record StartAgentWorkflowRequest(
    [Required, StringLength(500, MinimumLength = 20)] string Objective);
