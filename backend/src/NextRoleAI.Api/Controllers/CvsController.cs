using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextRoleAI.Api.Contracts.Cvs;
using NextRoleAI.Application.Authentication;
using NextRoleAI.Application.Cvs;

namespace NextRoleAI.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.JobSeekerOnly)]
[Route("api/cv")]
public sealed class CvsController(ICvService cvService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CvResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var cv = await cvService.GetAsync(GetUserId(), cancellationToken);
        return cv is null ? NotFound() : Ok(cv);
    }

    [HttpPost]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<CvResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var content = file.OpenReadStream();
        var result = await cvService.UploadAsync(
            GetUserId(),
            new CvUpload(file.FileName, file.ContentType, file.Length, content),
            cancellationToken);

        return result.Succeeded
            ? Ok(result.Cv)
            : BadRequest(new ProblemDetails
            {
                Title = "CV upload failed",
                Detail = result.Message,
                Status = StatusCodes.Status400BadRequest
            });
    }

    [HttpPut("profile")]
    [ProducesResponseType<CvResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmProfile(
        UpdateCvProfileRequest request,
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

        var result = await cvService.ConfirmProfileAsync(
            GetUserId(),
            new CvProfileUpdate(
                request.CandidateName,
                request.Email,
                request.Phone,
                request.Location,
                request.CurrentJobTitle,
                request.ProfessionalSummary,
                request.YearsExperience,
                request.Skills),
            cancellationToken);

        return result.Succeeded ? Ok(result.Cv) : NotFound();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken)
    {
        var deleted = await cvService.DeleteAsync(GetUserId(), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        throw new InvalidOperationException("The authenticated user identifier is missing.");
}
