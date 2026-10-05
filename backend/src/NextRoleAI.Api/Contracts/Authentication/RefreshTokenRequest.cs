using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Authentication;

public sealed record RefreshTokenRequest(
    [Required, MaxLength(512)] string RefreshToken);
