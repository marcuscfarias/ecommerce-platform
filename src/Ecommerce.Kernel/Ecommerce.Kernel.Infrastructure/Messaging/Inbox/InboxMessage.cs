namespace Ecommerce.Kernel.Infrastructure.Messaging.Inbox;

internal sealed class InboxMessage
{
    public Guid MessageId { get; init; }
    public string Consumer { get; init; } = string.Empty;
    public DateTimeOffset ProcessedOnUtc { get; init; }
}
