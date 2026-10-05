namespace NextRoleAI.Application.Cvs;

public interface ICvTextExtractor
{
    bool Supports(string extension);

    Task<string> ExtractAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default);
}
