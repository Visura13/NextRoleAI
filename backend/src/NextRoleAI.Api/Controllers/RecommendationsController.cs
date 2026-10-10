using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextRoleAI.Api.Contracts.Recommendations;
using NextRoleAI.Application.Authentication;
using NextRoleAI.Application.Recommendations;

namespace NextRoleAI.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.JobSeekerOnly)]
[Route("api/recommendations")]
public sealed class RecommendationsController(
    IRecommendationService recommendationService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<RecommendationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(
        [FromQuery, Range(1, 100)] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await recommendationService.GetAsync(
            GetUserId(),
            limit,
            cancellationToken);
        if (result.Succeeded)
        {
            return Ok(new RecommendationResponse(result.Items));
        }

        var status = result.Error == RecommendationError.ConfirmedCvRequired
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status503ServiceUnavailable;
        return StatusCode(status, new ProblemDetails
        {
            Title = result.Error == RecommendationError.ConfirmedCvRequired
                ? "Confirmed CV required"
                : "AI ranking unavailable",
            Detail = result.Message,
            Status = status
        });
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        throw new InvalidOperationException("The authenticated user identifier is missing.");
}
