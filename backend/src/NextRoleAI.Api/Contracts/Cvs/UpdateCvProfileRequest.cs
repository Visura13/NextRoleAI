using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Cvs;

public sealed record UpdateCvProfileRequest(
    [property: Required, StringLength(150)] string CandidateName,
    [property: EmailAddress, StringLength(254)] string Email,
    [property: StringLength(40)] string Phone,
    [property: StringLength(150)] string Location,
    [property: Required, StringLength(150)] string CurrentJobTitle,
    [property: Required, StringLength(2000, MinimumLength = 20)] string ProfessionalSummary,
    [property: Range(0, 80)] int YearsExperience,
    [property: Required, MinLength(1), MaxLength(50)] IReadOnlyCollection<string> Skills);
