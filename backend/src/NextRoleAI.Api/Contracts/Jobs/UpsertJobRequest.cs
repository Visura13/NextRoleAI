using System.ComponentModel.DataAnnotations;
using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Api.Contracts.Jobs;

public sealed record UpsertJobRequest(
    [property: Required, MaxLength(200)] string Title,
    [property: Required, MaxLength(8000)] string Description,
    [property: Required, MaxLength(150)] string Location,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    [property: Range(0, 80)] int MinimumYearsExperience,
    [property: Range(typeof(decimal), "0", "9999999999999999")] decimal? SalaryMinimum,
    [property: Range(typeof(decimal), "0", "9999999999999999")] decimal? SalaryMaximum,
    [property: RegularExpression("^[A-Za-z]{3}$")] string? SalaryCurrency,
    DateTimeOffset? ClosesAtUtc,
    [property: Required, MinLength(1), MaxLength(30)] IReadOnlyCollection<JobSkillRequest> Skills);
