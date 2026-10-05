using NextRoleAI.Infrastructure.Identity;

namespace NextRoleAI.Api.Tests.Identity;

public sealed class RefreshTokenTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsActive_ReturnsTrueForUnrevokedUnexpiredToken()
    {
        var token = CreateToken(Now.AddDays(1));

        Assert.True(token.IsActive(Now));
    }

    [Fact]
    public void Revoke_MakesTokenInactiveAndRecordsReplacement()
    {
        var token = CreateToken(Now.AddDays(1));

        token.Revoke(Now, "replacement-hash");

        Assert.False(token.IsActive(Now));
        Assert.Equal(Now, token.RevokedAtUtc);
        Assert.Equal("replacement-hash", token.ReplacedByTokenHash);
    }

    [Fact]
    public void IsActive_ReturnsFalseForExpiredToken()
    {
        var token = CreateToken(Now.AddSeconds(-1));

        Assert.False(token.IsActive(Now));
    }

    private static RefreshToken CreateToken(DateTimeOffset expiresAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid().ToString(),
            TokenHash = "token-hash",
            CreatedAtUtc = Now.AddDays(-1),
            ExpiresAtUtc = expiresAt
        };
}
