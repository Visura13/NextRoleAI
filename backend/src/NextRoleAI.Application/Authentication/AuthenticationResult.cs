namespace NextRoleAI.Application.Authentication;

public sealed record AuthenticationResult(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);
