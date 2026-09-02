using Ecommerce.Kernel.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.Infrastructure.Notifications;

internal sealed partial class ConsoleEmailSender(
    IOptions<EmailOptions> options,
    ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        LogEmail(logger, options.Value.Sender, message.To, message.Subject, message.HtmlBody);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Email from {Sender} to {Recipient} with subject {Subject} and body {HtmlBody}")]
    private static partial void LogEmail(
        ILogger logger,
        string sender,
        string recipient,
        string subject,
        string htmlBody);
}
