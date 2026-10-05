using NextRoleAI.Infrastructure.Identity;

namespace NextRoleAI.Infrastructure.Authentication;

internal sealed record GeneratedTokenPair(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    RefreshToken RefreshTokenEntity);
