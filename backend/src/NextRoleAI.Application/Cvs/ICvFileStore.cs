namespace NextRoleAI.Application.Cvs;

public interface ICvFileStore
{
    Task StoreAsync(
        string storageKey,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}
