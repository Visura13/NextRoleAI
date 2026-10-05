using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Profiles;

public sealed record UpdateCompanyProfileRequest(
    [property: Required, MaxLength(200)] string Name,
    [property: Required, MaxLength(3000)] string Description,
    [property: Required, MaxLength(150)] string Location,
    [property: Url, MaxLength(500)] string? WebsiteUrl);
