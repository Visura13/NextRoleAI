namespace NextRoleAI.Infrastructure.Identity;

public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public ApplicationUser User { get; set; } = null!;

    public bool IsActive(DateTimeOffset now) =>
        RevokedAtUtc is null && ExpiresAtUtc > now;

    public void Revoke(DateTimeOffset now, string? replacementTokenHash = null)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RevokedAtUtc = now;
        ReplacedByTokenHash = replacementTokenHash;
    }
}
