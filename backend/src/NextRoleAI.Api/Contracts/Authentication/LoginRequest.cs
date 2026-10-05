using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Authentication;

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(128)] string Password);
