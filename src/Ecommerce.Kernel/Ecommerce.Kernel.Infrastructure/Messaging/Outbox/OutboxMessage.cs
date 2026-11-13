namespace Ecommerce.Kernel.Infrastructure.Messaging.Outbox;

internal sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public string EventName { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public DateTimeOffset OccurredOnUtc { get; init; }
}
