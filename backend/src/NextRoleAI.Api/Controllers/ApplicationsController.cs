using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextRoleAI.Api.Contracts.Applications;
using NextRoleAI.Application.Applications;
using NextRoleAI.Application.Authentication;
using NextRoleAI.Domain.Applications;

namespace NextRoleAI.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.JobSeekerOnly)]
[Route("api/applications")]
public sealed class ApplicationsController(IApplicationService applicationService)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApplicationListResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ApplicationStatus? status,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (status is not null && !Enum.IsDefined(status.Value))
        {
            ModelState.AddModelError(nameof(status), "The application status is invalid.");
            return ValidationProblem(ModelState);
        }

        return Ok(await applicationService.GetForJobSeekerAsync(
            GetUserId(), status, page, pageSize, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<JobApplicationResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var application = await applicationService.GetForJobSeekerAsync(
            GetUserId(), id, cancellationToken);
        return application is null ? NotFound() : Ok(application);
    }

    [HttpPost]
    [ProducesResponseType<JobApplicationResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit(
        SubmitApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await applicationService.SubmitAsync(
            GetUserId(),
            new SubmitApplicationInput(
                request.JobPostingId,
                request.CoverNote,
                request.SourceWorkflowRunId),
            cancellationToken);

        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Application!.Id }, result.Application)
            : FromFailure(result);
    }

    [HttpPost("{id:guid}/withdraw")]
    [ProducesResponseType<JobApplicationResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Withdraw(
        Guid id,
        ApplicationNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await applicationService.WithdrawAsync(
            GetUserId(), id, request.Note, cancellationToken);
        return result.Succeeded ? Ok(result.Application) : FromFailure(result);
    }

    [HttpPost("{id:guid}/respond")]
    [ProducesResponseType<JobApplicationResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Respond(
        Guid id,
        ApplicationNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await applicationService.RespondAsync(
            GetUserId(), id, request.Note, cancellationToken);
        return result.Succeeded ? Ok(result.Application) : FromFailure(result);
    }

    private IActionResult FromFailure(ApplicationCommandResult result) =>
        result.Error == ApplicationCommandError.NotFound
            ? NotFound()
            : Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Application operation failed",
                detail: result.Message);

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        throw new InvalidOperationException("The authenticated user identifier is missing.");
}
