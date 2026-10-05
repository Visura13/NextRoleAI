using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextRoleAI.Api.Contracts.Jobs;
using NextRoleAI.Application.Authentication;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.RecruiterOnly)]
[Route("api/recruiter/jobs")]
public sealed class RecruiterJobsController(IJobService jobService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<JobResult>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await jobService.GetRecruiterJobsAsync(
            GetUserId(),
            page,
            pageSize,
            cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<JobResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var job = await jobService.GetRecruiterJobAsync(
            GetUserId(),
            id,
            cancellationToken);

        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost]
    [ProducesResponseType<JobResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        UpsertJobRequest request,
        CancellationToken cancellationToken)
    {
        if (!ValidateRequest(request))
        {
            return ValidationProblem(ModelState);
        }

        var result = await jobService.CreateAsync(
            GetUserId(),
            ToInput(request),
            cancellationToken);

        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Job!.Id }, result.Job)
            : FromFailure(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<JobResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        UpsertJobRequest request,
        CancellationToken cancellationToken)
    {
        if (!ValidateRequest(request))
        {
            return ValidationProblem(ModelState);
        }

        var result = await jobService.UpdateAsync(
            GetUserId(),
            id,
            ToInput(request),
            cancellationToken);

        return result.Succeeded ? Ok(result.Job) : FromFailure(result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<JobResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        ChangeJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Status))
        {
            ModelState.AddModelError(nameof(request.Status), "The job status is invalid.");
            return ValidationProblem(ModelState);
        }

        var result = await jobService.ChangeStatusAsync(
            GetUserId(),
            id,
            request.Status,
            cancellationToken);

        return result.Succeeded ? Ok(result.Job) : FromFailure(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await jobService.DeleteAsync(
            GetUserId(),
            id,
            cancellationToken);

        return result.Succeeded ? NoContent() : FromFailure(result);
    }

    private bool ValidateRequest(UpsertJobRequest request)
    {
        if (!Enum.IsDefined(request.EmploymentType))
        {
            ModelState.AddModelError(
                nameof(request.EmploymentType),
                "The employment type is invalid.");
        }

        if (!Enum.IsDefined(request.WorkMode))
        {
            ModelState.AddModelError(nameof(request.WorkMode), "The work mode is invalid.");
        }

        if (request.SalaryMinimum > request.SalaryMaximum)
        {
            ModelState.AddModelError(
                nameof(request.SalaryMaximum),
                "Maximum salary must be greater than or equal to minimum salary.");
        }

        if ((request.SalaryMinimum is not null || request.SalaryMaximum is not null) &&
            string.IsNullOrWhiteSpace(request.SalaryCurrency))
        {
            ModelState.AddModelError(
                nameof(request.SalaryCurrency),
                "Salary currency is required when a salary is provided.");
        }

        return ModelState.IsValid;
    }

    private static JobUpsert ToInput(UpsertJobRequest request) =>
        new(
            request.Title,
            request.Description,
            request.Location,
            request.EmploymentType,
            request.WorkMode,
            request.MinimumYearsExperience,
            request.SalaryMinimum,
            request.SalaryMaximum,
            request.SalaryCurrency,
            request.ClosesAtUtc,
            request.Skills
                .Select(skill => new JobSkillInput(skill.Name, skill.IsRequired))
                .ToArray());

    private IActionResult FromFailure(JobCommandResult result) =>
        result.Error == JobCommandError.NotFound
            ? NotFound()
            : Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Job operation failed",
                detail: result.Message);

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        throw new InvalidOperationException("The authenticated user identifier is missing.");
}
