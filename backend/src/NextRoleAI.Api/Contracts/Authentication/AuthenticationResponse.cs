using NextRoleAI.Application.Authentication;

namespace NextRoleAI.Api.Contracts.Authentication;

public sealed record AuthenticationResponse(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc)
{
    public static AuthenticationResponse FromResult(AuthenticationResult result) =>
        new(
            result.UserId,
            result.Email,
            result.FirstName,
            result.LastName,
            result.Role,
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc);
}
