namespace NextRoleAI.Infrastructure.Cvs;

internal interface ICvLanguageModelClient
{
    Task<string> GenerateJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}
