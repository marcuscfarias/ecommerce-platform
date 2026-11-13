using System.Text.Json;
using Ecommerce.Kernel.Application.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Kernel.Infrastructure.Messaging.Outbox;

public static class OutboxDbContextExtensions
{
    public static void AddIntegrationEvent<TEvent>(this DbContext db, TEvent @event)
        where TEvent : IIntegrationEvent
    {
        db.Set<OutboxMessage>().Add(new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            EventName = IntegrationEventName.Of<TEvent>(),
            Payload = JsonSerializer.Serialize(@event, JsonSerializerOptions.Web),
            OccurredOnUtc = DateTimeOffset.UtcNow,
        });
    }
}
