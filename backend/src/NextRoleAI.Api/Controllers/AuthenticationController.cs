using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextRoleAI.Api.Contracts.Authentication;
using NextRoleAI.Application.Authentication;

namespace NextRoleAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(IAuthenticationService authenticationService)
    : ControllerBase
{
    [HttpPost("register/job-seeker")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> RegisterJobSeeker(
        RegisterRequest request,
        CancellationToken cancellationToken) =>
        Register(request, RoleNames.JobSeeker, cancellationToken);

    [HttpPost("register/recruiter")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> RegisterRecruiter(
        RegisterRequest request,
        CancellationToken cancellationToken) =>
        Register(request, RoleNames.Recruiter, cancellationToken);

    [HttpPost("login")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var outcome = await authenticationService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        return outcome.Succeeded
            ? Ok(AuthenticationResponse.FromResult(outcome.Result!))
            : AuthenticationFailed(outcome);
    }

    [HttpPost("refresh")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var outcome = await authenticationService.RefreshAsync(
            request.RefreshToken,
            cancellationToken);

        return outcome.Succeeded
            ? Ok(AuthenticationResponse.FromResult(outcome.Result!))
            : AuthenticationFailed(outcome);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await authenticationService.RevokeRefreshTokenAsync(
            request.RefreshToken,
            cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var currentUser = await authenticationService.GetCurrentUserAsync(
            userId,
            cancellationToken);

        return currentUser is null
            ? NotFound()
            : Ok(CurrentUserResponse.FromResult(currentUser));
    }

    private async Task<IActionResult> Register(
        RegisterRequest request,
        string role,
        CancellationToken cancellationToken)
    {
        var outcome = await authenticationService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            role,
            cancellationToken);

        if (!outcome.Succeeded)
        {
            return ValidationFailure(outcome);
        }

        var response = AuthenticationResponse.FromResult(outcome.Result!);
        return Created("/api/auth/me", response);
    }

    private ObjectResult AuthenticationFailed(AuthenticationOutcome outcome) =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Authentication failed",
            detail: outcome.Errors.FirstOrDefault()?.Description ?? "Authentication failed.");

    private IActionResult ValidationFailure(AuthenticationOutcome outcome)
    {
        var errors = outcome.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());

        return ValidationProblem(
            new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Registration failed"
            });
    }
}
