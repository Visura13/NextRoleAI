using NextRoleAI.Domain.Jobs;

namespace NextRoleAI.Application.Jobs;

public interface IJobService
{
    Task<JobCommandResult> CreateAsync(
        string recruiterUserId,
        JobUpsert input,
        CancellationToken cancellationToken = default);

    Task<PagedResult<JobResult>> GetRecruiterJobsAsync(
        string recruiterUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<JobResult?> GetRecruiterJobAsync(
        string recruiterUserId,
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<JobCommandResult> UpdateAsync(
        string recruiterUserId,
        Guid jobId,
        JobUpsert input,
        CancellationToken cancellationToken = default);

    Task<JobCommandResult> ChangeStatusAsync(
        string recruiterUserId,
        Guid jobId,
        JobStatus status,
        CancellationToken cancellationToken = default);

    Task<JobCommandResult> DeleteAsync(
        string recruiterUserId,
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<JobResult>> SearchPublishedAsync(
        JobSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<JobResult?> GetPublishedAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);
}
