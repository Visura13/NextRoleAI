using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NextRoleAI.Application.Authentication;
using NextRoleAI.Infrastructure.Identity;
using NextRoleAI.Infrastructure.Persistence;

namespace NextRoleAI.Infrastructure.Authentication;

internal sealed class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    JwtTokenGenerator tokenGenerator,
    TimeProvider timeProvider) : IAuthenticationService
{
    private static readonly AuthenticationError InvalidCredentials =
        new("InvalidCredentials", "The email or password is incorrect.");

    private static readonly AuthenticationError InvalidRefreshToken =
        new("InvalidRefreshToken", "The refresh token is invalid or has expired.");

    public async Task<AuthenticationOutcome> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        CancellationToken cancellationToken = default)
    {
        if (!RoleNames.All.Contains(role, StringComparer.Ordinal))
        {
            return AuthenticationOutcome.Failure(
                new AuthenticationError("InvalidRole", "The requested role is not supported."));
        }

        IDbContextTransaction? transaction = null;
        if (dbContext.Database.IsRelational())
        {
            transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        try
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = email.Trim(),
                UserName = email.Trim(),
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                CreatedAtUtc = timeProvider.GetUtcNow()
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return FailureFromIdentity(createResult);
            }

            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                return FailureFromIdentity(roleResult);
            }

            var result = await IssueTokenPairAsync(user, role, cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return AuthenticationOutcome.Success(result);
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public async Task<AuthenticationOutcome> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null || !user.IsActive || !await userManager.CheckPasswordAsync(user, password))
        {
            return AuthenticationOutcome.Failure(InvalidCredentials);
        }

        var role = await GetSingleRoleAsync(user);
        var result = await IssueTokenPairAsync(user, role, cancellationToken);
        return AuthenticationOutcome.Success(result);
    }

    public async Task<AuthenticationOutcome> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = JwtTokenGenerator.HashRefreshToken(refreshToken);
        var storedToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.User.IsActive)
        {
            return AuthenticationOutcome.Failure(InvalidRefreshToken);
        }

        var now = timeProvider.GetUtcNow();
        if (!storedToken.IsActive(now))
        {
            if (storedToken.RevokedAtUtc is not null && storedToken.ReplacedByTokenHash is not null)
            {
                await RevokeAllActiveTokensAsync(storedToken.UserId, now, cancellationToken);
            }

            return AuthenticationOutcome.Failure(InvalidRefreshToken);
        }

        var role = await GetSingleRoleAsync(storedToken.User);
        var generatedTokens = tokenGenerator.Generate(storedToken.User, role);
        storedToken.Revoke(now, generatedTokens.RefreshTokenEntity.TokenHash);
        dbContext.RefreshTokens.Add(generatedTokens.RefreshTokenEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return AuthenticationOutcome.Success(ToAuthenticationResult(storedToken.User, role, generatedTokens));
    }

    public async Task RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = JwtTokenGenerator.HashRefreshToken(refreshToken);
        var storedToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive(timeProvider.GetUtcNow()))
        {
            return;
        }

        storedToken.Revoke(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CurrentUserResult?> GetCurrentUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var role = await GetSingleRoleAsync(user);
        return new CurrentUserResult(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            role);
    }

    private async Task<AuthenticationResult> IssueTokenPairAsync(
        ApplicationUser user,
        string role,
        CancellationToken cancellationToken)
    {
        var generatedTokens = tokenGenerator.Generate(user, role);
        dbContext.RefreshTokens.Add(generatedTokens.RefreshTokenEntity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToAuthenticationResult(user, role, generatedTokens);
    }

    private async Task<string> GetSingleRoleAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return roles.Single();
    }

    private async Task RevokeAllActiveTokensAsync(
        string userId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var activeTokens = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null && token.ExpiresAtUtc > now)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.Revoke(now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static AuthenticationOutcome FailureFromIdentity(IdentityResult result) =>
        AuthenticationOutcome.Failure(
            result.Errors
                .Select(error => new AuthenticationError(error.Code, error.Description))
                .ToArray());

    private static AuthenticationResult ToAuthenticationResult(
        ApplicationUser user,
        string role,
        GeneratedTokenPair tokens) =>
        new(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            role,
            tokens.AccessToken,
            tokens.AccessTokenExpiresAtUtc,
            tokens.RefreshToken,
            tokens.RefreshTokenEntity.ExpiresAtUtc);
}
