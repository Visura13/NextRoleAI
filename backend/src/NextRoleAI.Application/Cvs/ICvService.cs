namespace NextRoleAI.Application.Cvs;

public interface ICvService
{
    Task<CvResult?> GetAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<CvCommandResult> UploadAsync(
        string userId,
        CvUpload upload,
        CancellationToken cancellationToken = default);

    Task<CvCommandResult> ConfirmProfileAsync(
        string userId,
        CvProfileUpdate update,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
