using Microsoft.Extensions.Options;
using NextRoleAI.Application.Cvs;

namespace NextRoleAI.Infrastructure.Cvs;

internal sealed class LocalCvFileStore : ICvFileStore
{
    private readonly string rootPath;

    public LocalCvFileStore(IOptions<CvStorageOptions> options)
    {
        var configured = options.Value.RootPath;
        rootPath = Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NextRoleAI",
                "cvs")
            : configured);
        Directory.CreateDirectory(rootPath);
    }

    public async Task StoreAsync(
        string storageKey,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var path = Resolve(storageKey);
        await using var destination = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await content.CopyToAsync(destination, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(
            Resolve(storageKey),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Resolve(storageKey);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string Resolve(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) ||
            storageKey.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new InvalidOperationException("The CV storage key is invalid.");
        }

        var path = Path.GetFullPath(Path.Combine(rootPath, storageKey));
        var pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!path.StartsWith(rootPath + Path.DirectorySeparatorChar, pathComparison) &&
            !string.Equals(path, rootPath, pathComparison))
        {
            throw new InvalidOperationException("The CV storage path escaped its root.");
        }

        return path;
    }
}
