using Microsoft.AspNetCore.Identity;

namespace NextRoleAI.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; } = [];
}
