using Microsoft.Extensions.Logging;

namespace Ecommerce.Kernel.Infrastructure.Messaging;

internal static partial class MessagingLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Connected to the message broker")]
    public static partial void BrokerConnected(ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Could not connect to the message broker; retrying in {RetryDelay}")]
    public static partial void BrokerConnectFailed(ILogger logger, Exception exception, TimeSpan retryDelay);
}
