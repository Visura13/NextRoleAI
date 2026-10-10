using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace NextRoleAI.Infrastructure.Cvs;

internal sealed class OpenAiCompatibleCvLanguageModelClient :
    ICvLanguageModelClient,
    IDisposable
{
    private readonly CvAiOptions options;
    private readonly HttpClient httpClient;

    public OpenAiCompatibleCvLanguageModelClient(IOptions<CvAiOptions> options)
    {
        this.options = options.Value;
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(EnsureTrailingSlash(this.options.BaseUrl)),
            Timeout = TimeSpan.FromSeconds(this.options.TimeoutSeconds)
        };
    }

    public async Task<string> GenerateJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(new
            {
                model = options.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                response_format = new { type = "json_object" }
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"CV AI provider returned HTTP {(int)response.StatusCode}.");
        }

        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("CV AI provider returned an empty response.");
        var choices = payload.RootElement.GetProperty("choices");
        if (choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("CV AI provider returned no completion choices.");
        }

        var content = choices[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
        return string.IsNullOrWhiteSpace(content)
            ? throw new InvalidOperationException("CV AI provider returned empty JSON content.")
            : content;
    }

    public void Dispose() => httpClient.Dispose();

    private static string EnsureTrailingSlash(string value) =>
        value.EndsWith("/", StringComparison.Ordinal) ? value : value + "/";
}
