namespace NextRoleAI.Application.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationOutcome> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        CancellationToken cancellationToken = default);

    Task<AuthenticationOutcome> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticationOutcome> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<CurrentUserResult?> GetCurrentUserAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
