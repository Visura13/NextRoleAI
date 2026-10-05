using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextRoleAI.Api.Contracts.AgentWorkflows;
using NextRoleAI.Application.AgentWorkflows;
using NextRoleAI.Application.Authentication;
using NextRoleAI.Domain.AgentWorkflows;

namespace NextRoleAI.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.JobSeekerOnly)]
[Route("api/agent-workflows")]
public sealed class AgentWorkflowsController(IAgentWorkflowService workflowService)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AgentWorkflowResult>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Start(
        StartAgentWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var result = await workflowService.StartAsync(
            GetUserId(),
            new StartAgentWorkflow(request.Objective),
            cancellationToken);
        return CreatedAtAction(
            nameof(Get),
            new { workflowId = result.Workflow!.Id },
            result.Workflow);
    }

    [HttpGet]
    [ProducesResponseType<AgentWorkflowListResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] AgentWorkflowStatus? status = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 50)] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await workflowService.ListAsync(
            GetUserId(),
            new AgentWorkflowQuery(status, page, pageSize),
            cancellationToken));

    [HttpGet("{workflowId:guid}")]
    [ProducesResponseType<AgentWorkflowResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        Guid workflowId,
        CancellationToken cancellationToken)
    {
        var workflow = await workflowService.GetAsync(
            GetUserId(),
            workflowId,
            cancellationToken);
        return workflow is null ? NotFound() : Ok(workflow);
    }

    [HttpPost("{workflowId:guid}/decision")]
    [ProducesResponseType<AgentWorkflowResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Decide(
        Guid workflowId,
        DecideAgentWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var result = await workflowService.DecideAsync(
            GetUserId(),
            workflowId,
            new AgentWorkflowDecision(
                request.Decision!.Value,
                request.Feedback,
                request.RevisedObjective),
            cancellationToken);
        return result.Error switch
        {
            AgentWorkflowCommandError.None => Ok(result.Workflow),
            AgentWorkflowCommandError.NotFound => NotFound(),
            AgentWorkflowCommandError.InvalidState => Conflict(new ProblemDetails
            {
                Title = "Workflow is not awaiting approval",
                Detail = result.Message,
                Status = StatusCodes.Status409Conflict
            }),
            _ => BadRequest(new ProblemDetails
            {
                Title = "Invalid workflow decision",
                Detail = result.Message,
                Status = StatusCodes.Status400BadRequest
            })
        };
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        throw new InvalidOperationException("The authenticated user identifier is missing.");
}
