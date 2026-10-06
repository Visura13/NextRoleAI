using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NextRoleAI.Api.Contracts.Authentication;
using NextRoleAI.IntegrationTests.Infrastructure;

namespace NextRoleAI.IntegrationTests.Quality;

public sealed class OpenApiAndSecurityEndpointsTests(NextRoleAIApiFactory factory)
    : IClassFixture<NextRoleAIApiFactory>
{
    private const string ValidPassword = "StrongPassword1!";

    [PostgresFact]
    public async Task OpenApiDocument_IsPublicAndDescribesBearerProtectedApi()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.Equal("NextRoleAI API", root.GetProperty("info").GetProperty("title").GetString());
        Assert.True(root.GetProperty("paths").TryGetProperty("/api/jobs", out _));
        Assert.True(root.GetProperty("paths").TryGetProperty("/api/agent-workflows", out _));
        Assert.Equal(
            "http",
            root.GetProperty("components")
                .GetProperty("securitySchemes")
                .GetProperty("bearer")
                .GetProperty("type")
                .GetString());
    }

    [PostgresFact]
    public async Task ProtectedEndpoints_EnforceAuthenticationAndRoleAuthorization()
    {
        using var client = factory.CreateClient();

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/auth/me")).StatusCode);

        var registrationResponse = await client.PostAsJsonAsync(
            "/api/auth/register/job-seeker",
            new RegisterRequest(
                $"security-{Guid.NewGuid():N}@example.test",
                ValidPassword,
                "Security",
                "Test"));
        registrationResponse.EnsureSuccessStatusCode();
        var registration = await registrationResponse.Content
            .ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(registration);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", registration.AccessToken);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await client.GetAsync("/api/recruiter/jobs")).StatusCode);
    }
}
