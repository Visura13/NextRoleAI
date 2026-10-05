using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Profiles;

public sealed record UpdateCompanyProfileRequest(
    [Required, MaxLength(200)] string Name,
    [Required, MaxLength(3000)] string Description,
    [Required, MaxLength(150)] string Location,
    [Url, MaxLength(500)] string? WebsiteUrl);
