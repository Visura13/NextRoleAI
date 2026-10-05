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
    public async Task<IActionResult> Get(
        [FromQuery, Range(1, 100)] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await recommendationService.GetAsync(
            GetUserId(),
            limit,
            cancellationToken);
        return result.Succeeded
            ? Ok(new RecommendationResponse(result.Items))
            : Conflict(new ProblemDetails
            {
                Title = "Confirmed CV required",
                Detail = result.Message,
                Status = StatusCodes.Status409Conflict
            });
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        throw new InvalidOperationException("The authenticated user identifier is missing.");
}
