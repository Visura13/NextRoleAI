using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextRoleAI.Application.Jobs;
using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/jobs")]
public sealed class JobsController(IJobService jobService) : ControllerBase
{
    private static readonly string[] SupportedSorts = ["newest", "title", "closingDate"];

    [HttpGet]
    [ProducesResponseType<PagedResult<JobResult>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] string? location,
        [FromQuery] EmploymentType? employmentType,
        [FromQuery] WorkMode? workMode,
        [FromQuery] string? skill,
        [FromQuery] string sortBy = "newest",
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (employmentType is not null && !Enum.IsDefined(employmentType.Value))
        {
            ModelState.AddModelError(
                nameof(employmentType),
                "The employment type is invalid.");
        }

        if (workMode is not null && !Enum.IsDefined(workMode.Value))
        {
            ModelState.AddModelError(nameof(workMode), "The work mode is invalid.");
        }

        if (!SupportedSorts.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(sortBy),
                "Sort must be one of: newest, title, closingDate.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await jobService.SearchPublishedAsync(
            new JobSearchQuery(
                search,
                location,
                employmentType,
                workMode,
                skill,
                sortBy,
                page,
                pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<JobResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var job = await jobService.GetPublishedAsync(id, cancellationToken);
        return job is null ? NotFound() : Ok(job);
    }
}
