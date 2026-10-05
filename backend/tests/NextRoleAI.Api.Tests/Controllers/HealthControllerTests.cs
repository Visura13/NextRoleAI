using Microsoft.AspNetCore.Mvc;
using NextRoleAI.Api.Contracts.Health;
using NextRoleAI.Api.Controllers;

namespace NextRoleAI.Api.Tests.Controllers;

public sealed class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsHealthyApiResponse()
    {
        var beforeRequest = DateTimeOffset.UtcNow;
        var controller = new HealthController();

        var result = controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiHealthResponse>(okResult.Value);
        Assert.Equal("NextRoleAI.Api", response.Service);
        Assert.Equal("Healthy", response.Status);
        Assert.InRange(response.UtcTimestamp, beforeRequest, DateTimeOffset.UtcNow);
    }
}
