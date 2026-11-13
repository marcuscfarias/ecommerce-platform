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

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Could not publish outbox message {MessageId} ({EventName}); it stays in the outbox")]
    public static partial void RelayPublishFailed(ILogger logger, Exception exception, Guid messageId, string eventName);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Outbox relay tick failed for {DbContext}")]
    public static partial void RelayTickFailed(ILogger logger, Exception exception, string dbContext);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Warning,
        Message = "Message {MessageId} ({EventName}) on {Queue} was dead-lettered after {Attempts} attempt(s)")]
    public static partial void MessageDeadLettered(
        ILogger logger,
        Exception exception,
        Guid messageId,
        string eventName,
        string queue,
        int attempts);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Message {MessageId} on {Queue} was dead-lettered unread: missing id or unreadable body")]
    public static partial void MessageRejected(ILogger logger, string? messageId, string queue);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Host stopping: message {MessageId} on {Queue} is left for redelivery")]
    public static partial void MessageLeftForRedelivery(ILogger logger, Guid messageId, string queue);
}
