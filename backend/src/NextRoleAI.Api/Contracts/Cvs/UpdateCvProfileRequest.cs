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
    [Required, MinLength(1), MaxLength(50)] IReadOnlyCollection<string> Skills,
    [MaxLength(20)] IReadOnlyCollection<CvEducationRequest>? Education = null);

public sealed record CvEducationRequest(
    [Required, StringLength(150)] string Qualification,
    [StringLength(200)] string FieldOfStudy = "",
    [StringLength(200)] string Institution = "",
    [StringLength(50)] string Status = "");
