using NextRoleAI.Domain.Applications;

namespace NextRoleAI.Application.Applications;

public interface IApplicationService
{
    Task<ApplicationCommandResult> SubmitAsync(
        string jobSeekerUserId,
        SubmitApplicationInput input,
        CancellationToken cancellationToken = default);

    Task<ApplicationListResult> GetForJobSeekerAsync(
        string jobSeekerUserId,
        ApplicationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<JobApplicationResult?> GetForJobSeekerAsync(
        string jobSeekerUserId,
        Guid applicationId,
        CancellationToken cancellationToken = default);

    Task<ApplicationCommandResult> WithdrawAsync(
        string jobSeekerUserId,
        Guid applicationId,
        string note,
        CancellationToken cancellationToken = default);

    Task<ApplicationCommandResult> RespondAsync(
        string jobSeekerUserId,
        Guid applicationId,
        string note,
        CancellationToken cancellationToken = default);

    Task<ApplicationListResult> GetForRecruiterAsync(
        string recruiterUserId,
        ApplicationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<JobApplicationResult?> GetForRecruiterAsync(
        string recruiterUserId,
        Guid applicationId,
        CancellationToken cancellationToken = default);

    Task<ApplicationCommandResult> DecideAsync(
        string recruiterUserId,
        Guid applicationId,
        ApplicationStatus status,
        string note,
        CancellationToken cancellationToken = default);
}
