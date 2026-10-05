using System.ComponentModel.DataAnnotations;
using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Api.Contracts.Jobs;

public sealed record UpsertJobRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(8000)] string Description,
    [Required, MaxLength(150)] string Location,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    [Range(0, 80)] int MinimumYearsExperience,
    [Range(typeof(decimal), "0", "9999999999999999")] decimal? SalaryMinimum,
    [Range(typeof(decimal), "0", "9999999999999999")] decimal? SalaryMaximum,
    [RegularExpression("^[A-Za-z]{3}$")] string? SalaryCurrency,
    DateTimeOffset? ClosesAtUtc,
    [Required, MinLength(1), MaxLength(30)] IReadOnlyCollection<JobSkillRequest> Skills);
