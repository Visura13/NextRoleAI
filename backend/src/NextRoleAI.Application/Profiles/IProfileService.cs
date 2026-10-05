namespace NextRoleAI.Application.Profiles;

public interface IProfileService
{
    Task<JobSeekerProfileResult?> GetJobSeekerAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<JobSeekerProfileResult> UpsertJobSeekerAsync(
        string userId,
        JobSeekerProfileUpdate update,
        CancellationToken cancellationToken = default);

    Task<CompanyProfileResult?> GetCompanyAsync(
        string recruiterUserId,
        CancellationToken cancellationToken = default);

    Task<CompanyProfileResult> UpsertCompanyAsync(
        string recruiterUserId,
        CompanyProfileUpdate update,
        CancellationToken cancellationToken = default);
}
