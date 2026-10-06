using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextRoleAI.Application.Applications;

namespace NextRoleAI.Infrastructure.Applications;

internal sealed class ResendApplicationNotificationSender(
    IOptions<NotificationOptions> options,
    ILogger<ResendApplicationNotificationSender> logger)
    : IApplicationNotificationSender, IDisposable
{
    private readonly NotificationOptions options = options.Value;
    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("https://api.resend.com/"),
        Timeout = TimeSpan.FromSeconds(10)
    };

    public async Task<NotificationSendResult> SendAsync(
        ApplicationNotification notification,
        CancellationToken cancellationToken = default)
    {
        if (!options.Enabled ||
            string.IsNullOrWhiteSpace(options.ResendApiKey) ||
            string.IsNullOrWhiteSpace(options.FromAddress))
        {
            return NotificationSendResult.NotConfigured(
                "Resend",
                "Email delivery is disabled or not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(new
            {
                from = options.FromAddress,
                to = new[] { notification.RecipientEmail },
                subject = notification.Subject,
                text = notification.PlainTextBody
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            options.ResendApiKey);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Resend rejected an application notification with status {StatusCode}.",
                    (int)response.StatusCode);
                return NotificationSendResult.Failed(
                    "Resend",
                    $"Provider returned HTTP {(int)response.StatusCode}.");
            }

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>(
                cancellationToken: cancellationToken);
            var providerMessageId = payload.TryGetProperty("id", out var id)
                ? id.GetString()
                : null;
            return NotificationSendResult.Delivered("Resend", providerMessageId);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return NotificationSendResult.Failed("Resend", "Provider request timed out.");
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(
                exception,
                "Resend application notification delivery failed.");
            return NotificationSendResult.Failed(
                "Resend",
                "Provider request failed.");
        }
    }

    public void Dispose() => httpClient.Dispose();
}
