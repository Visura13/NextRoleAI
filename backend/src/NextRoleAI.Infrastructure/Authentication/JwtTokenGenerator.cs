using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NextRoleAI.Infrastructure.Identity;

namespace NextRoleAI.Infrastructure.Authentication;

internal sealed class JwtTokenGenerator(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider)
{
    private readonly JwtOptions _options = options.Value;

    public GeneratedTokenPair Generate(ApplicationUser user, string role)
    {
        var now = timeProvider.GetUtcNow();
        var accessTokenExpiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        var refreshTokenExpiresAt = now.AddDays(_options.RefreshTokenDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Role, role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: accessTokenExpiresAt.UtcDateTime,
            signingCredentials: credentials);

        var rawRefreshToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        var refreshTokenHash = HashRefreshToken(rawRefreshToken);

        return new GeneratedTokenPair(
            new JwtSecurityTokenHandler().WriteToken(token),
            accessTokenExpiresAt,
            rawRefreshToken,
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                CreatedAtUtc = now,
                ExpiresAtUtc = refreshTokenExpiresAt
            });
    }

    public static string HashRefreshToken(string refreshToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hash);
    }
}
