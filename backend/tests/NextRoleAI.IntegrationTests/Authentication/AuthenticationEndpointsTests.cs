using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NextRoleAI.Api.Contracts.Authentication;
using NextRoleAI.IntegrationTests.Infrastructure;

namespace NextRoleAI.IntegrationTests.Authentication;

public sealed class AuthenticationEndpointsTests(NextRoleAIApiFactory factory)
    : IClassFixture<NextRoleAIApiFactory>
{
    private const string ValidPassword = "StrongPassword1!";

    [PostgresFact]
    public async Task JobSeeker_CanRegisterAuthenticateRefreshAndLogout()
    {
        using var client = factory.CreateClient();
        var email = UniqueEmail("jobseeker");

        var registrationResponse = await client.PostAsJsonAsync(
            "/api/auth/register/job-seeker",
            new RegisterRequest(email, ValidPassword, "Test", "Seeker"));

        Assert.Equal(HttpStatusCode.Created, registrationResponse.StatusCode);
        var registration = await registrationResponse.Content
            .ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(registration);
        Assert.Equal("JobSeeker", registration.Role);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", registration.AccessToken);
        var currentUser = await client.GetFromJsonAsync<CurrentUserResponse>("/api/auth/me");
        Assert.NotNull(currentUser);
        Assert.Equal(email, currentUser.Email);
        Assert.Equal("JobSeeker", currentUser.Role);

        client.DefaultRequestHeaders.Authorization = null;
        var refreshResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(registration.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(refreshed);
        Assert.NotEqual(registration.RefreshToken, refreshed.RefreshToken);

        var reusedTokenResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(registration.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, reusedTokenResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, ValidPassword));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var login = await loginResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(login);

        var logoutResponse = await client.PostAsJsonAsync(
            "/api/auth/logout",
            new RefreshTokenRequest(login.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var revokedTokenResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshTokenRequest(login.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, revokedTokenResponse.StatusCode);
    }

    [PostgresFact]
    public async Task RecruiterRegistration_AssignsRecruiterRole()
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/register/recruiter",
            new RegisterRequest(
                UniqueEmail("recruiter"),
                ValidPassword,
                "Test",
                "Recruiter"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var registration = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(registration);
        Assert.Equal("Recruiter", registration.Role);
    }

    [PostgresFact]
    public async Task DuplicateEmail_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient();
        var email = UniqueEmail("duplicate");
        var request = new RegisterRequest(email, ValidPassword, "Duplicate", "User");

        var firstResponse = await client.PostAsJsonAsync(
            "/api/auth/register/job-seeker",
            request);
        var duplicateResponse = await client.PostAsJsonAsync(
            "/api/auth/register/job-seeker",
            request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    private static string UniqueEmail(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}@example.test";
}
