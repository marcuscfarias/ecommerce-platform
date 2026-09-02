using System.Net.Http.Json;
using Ecommerce.Kernel.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.Infrastructure.Notifications;

internal sealed partial class ResendEmailSender(
    HttpClient httpClient,
    IOptions<EmailOptions> options,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    internal const string SendPath = "/emails";

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(message.To))
        {
            LogEmptyRecipient(logger);
            return;
        }

        var payload = new ResendEmailRequest(
            options.Value.Sender,
            message.To,
            message.Subject,
            message.HtmlBody,
            message.TextBody);

        try
        {
            var response = await httpClient.PostAsJsonAsync(SendPath, payload, ct);

            if (!response.IsSuccessStatusCode)
            {
                LogRejected(logger, (int)response.StatusCode);
            }
        }
        catch (Exception exception)
        {
            LogRequestFailed(logger, exception);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Resend rejected the email with status {StatusCode}")]
    private static partial void LogRejected(ILogger logger, int statusCode);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "The request to Resend failed")]
    private static partial void LogRequestFailed(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "The email was not sent because the recipient is empty")]
    private static partial void LogEmptyRecipient(ILogger logger);
}
