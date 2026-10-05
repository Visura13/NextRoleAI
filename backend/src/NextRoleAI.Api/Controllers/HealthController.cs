using Microsoft.AspNetCore.Mvc;
using NextRoleAI.Api.Contracts.Health;

namespace NextRoleAI.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiHealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<ApiHealthResponse> Get()
    {
        var response = new ApiHealthResponse(
            Service: "NextRoleAI.Api",
            Status: "Healthy",
            UtcTimestamp: DateTimeOffset.UtcNow);

        return Ok(response);
    }
}
