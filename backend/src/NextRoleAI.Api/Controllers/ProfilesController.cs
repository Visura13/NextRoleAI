using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextRoleAI.Api.Contracts.Profiles;
using NextRoleAI.Application.Authentication;
using NextRoleAI.Application.Profiles;

namespace NextRoleAI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profiles")]
public sealed class ProfilesController(IProfileService profileService) : ControllerBase
{
    [HttpGet("job-seeker")]
    [Authorize(Policy = AuthorizationPolicies.JobSeekerOnly)]
    [ProducesResponseType<JobSeekerProfileResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobSeeker(CancellationToken cancellationToken)
    {
        var profile = await profileService.GetJobSeekerAsync(
            GetUserId(),
            cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("job-seeker")]
    [Authorize(Policy = AuthorizationPolicies.JobSeekerOnly)]
    [ProducesResponseType<JobSeekerProfileResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertJobSeeker(
        UpdateJobSeekerProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Skills.Any(skill =>
                string.IsNullOrWhiteSpace(skill) || skill.Trim().Length > 100))
        {
            ModelState.AddModelError(
                nameof(request.Skills),
                "Each skill must contain between 1 and 100 characters.");
            return ValidationProblem(ModelState);
        }

        var profile = await profileService.UpsertJobSeekerAsync(
            GetUserId(),
            new JobSeekerProfileUpdate(
                request.Headline,
                request.Summary,
                request.Location,
                request.PreferredJobTitle,
                request.PreferredSalary,
                request.YearsOfExperience,
                request.Skills),
            cancellationToken);

        return Ok(profile);
    }

    [HttpGet("company")]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOnly)]
    [ProducesResponseType<CompanyProfileResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompany(CancellationToken cancellationToken)
    {
        var profile = await profileService.GetCompanyAsync(
            GetUserId(),
            cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("company")]
    [Authorize(Policy = AuthorizationPolicies.RecruiterOnly)]
    [ProducesResponseType<CompanyProfileResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertCompany(
        UpdateCompanyProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await profileService.UpsertCompanyAsync(
            GetUserId(),
            new CompanyProfileUpdate(
                request.Name,
                request.Description,
                request.Location,
                request.WebsiteUrl),
            cancellationToken);

        return Ok(profile);
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        throw new InvalidOperationException("The authenticated user identifier is missing.");
}
