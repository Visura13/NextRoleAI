using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Profiles;

public sealed record UpdateJobSeekerProfileRequest(
    [Required, MaxLength(160)] string Headline,
    [Required, MaxLength(2000)] string Summary,
    [Required, MaxLength(150)] string Location,
    [Required, MaxLength(150)] string PreferredJobTitle,
    [Range(typeof(decimal), "1", "1000000000")] decimal? PreferredSalary,
    [Range(0, 80)] int YearsOfExperience,
    [MaxLength(30)] IReadOnlyCollection<string> Skills);
