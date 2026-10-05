using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Application.Jobs;

public sealed record JobSkillInput(string Name, bool IsRequired);

public sealed record JobUpsert(
    string Title,
    string Description,
    string Location,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    int MinimumYearsExperience,
    decimal? SalaryMinimum,
    decimal? SalaryMaximum,
    string? SalaryCurrency,
    DateTimeOffset? ClosesAtUtc,
    IReadOnlyCollection<JobSkillInput> Skills);

public sealed record JobResult(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string Title,
    string Description,
    string Location,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    int MinimumYearsExperience,
    decimal? SalaryMinimum,
    decimal? SalaryMaximum,
    string? SalaryCurrency,
    JobStatus Status,
    DateTimeOffset? PublishedAtUtc,
    DateTimeOffset? ClosesAtUtc,
    IReadOnlyCollection<JobSkillInput> Skills,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record JobSearchQuery(
    string? Search,
    string? Location,
    EmploymentType? EmploymentType,
    WorkMode? WorkMode,
    string? Skill,
    string SortBy,
    int Page,
    int PageSize);

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling((double)TotalCount / PageSize);
}

public enum JobCommandError
{
    None,
    NotFound,
    CompanyProfileRequired,
    InvalidTransition,
    PublishedJobCannotBeDeleted
}

public sealed record JobCommandResult(
    JobResult? Job,
    JobCommandError Error = JobCommandError.None,
    string? Message = null)
{
    public bool Succeeded => Error == JobCommandError.None;

    public static JobCommandResult Success(JobResult job) => new(job);

    public static JobCommandResult Failure(JobCommandError error, string message) =>
        new(null, error, message);
}
