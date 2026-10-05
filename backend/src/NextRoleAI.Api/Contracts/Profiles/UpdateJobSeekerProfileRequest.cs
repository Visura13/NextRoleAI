using System.ComponentModel.DataAnnotations;

namespace NextRoleAI.Api.Contracts.Profiles;

public sealed record UpdateJobSeekerProfileRequest(
    [property: Required, MaxLength(160)] string Headline,
    [property: Required, MaxLength(2000)] string Summary,
    [property: Required, MaxLength(150)] string Location,
    [property: Required, MaxLength(150)] string PreferredJobTitle,
    [property: Range(0, 80)] int YearsOfExperience,
    [property: MaxLength(30)] IReadOnlyCollection<string> Skills);
