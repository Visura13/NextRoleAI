namespace NextRoleAI.Application.Profiles;

public sealed record JobSeekerProfileUpdate(
    string Headline,
    string Summary,
    string Location,
    string PreferredJobTitle,
    decimal? PreferredSalary,
    int YearsOfExperience,
    IReadOnlyCollection<string> Skills);

public sealed record JobSeekerProfileResult(
    Guid Id,
    string Headline,
    string Summary,
    string Location,
    string PreferredJobTitle,
    decimal? PreferredSalary,
    int YearsOfExperience,
    IReadOnlyCollection<string> Skills,
    DateTimeOffset UpdatedAtUtc);

public sealed record CompanyProfileUpdate(
    string Name,
    string Description,
    string Location,
    string? WebsiteUrl);

public sealed record CompanyProfileResult(
    Guid Id,
    string Name,
    string Description,
    string Location,
    string? WebsiteUrl,
    DateTimeOffset UpdatedAtUtc);
