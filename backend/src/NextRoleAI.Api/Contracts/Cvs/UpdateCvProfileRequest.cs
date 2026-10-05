using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Cvs;

public sealed record UpdateCvProfileRequest(
    [Required, StringLength(150)] string CandidateName,
    [EmailAddress, StringLength(254)] string Email,
    [StringLength(40)] string Phone,
    [StringLength(150)] string Location,
    [Required, StringLength(150)] string CurrentJobTitle,
    [Required, StringLength(2000, MinimumLength = 20)] string ProfessionalSummary,
    [Range(0, 80)] int YearsExperience,
    [Required, MinLength(1), MaxLength(50)] IReadOnlyCollection<string> Skills);
