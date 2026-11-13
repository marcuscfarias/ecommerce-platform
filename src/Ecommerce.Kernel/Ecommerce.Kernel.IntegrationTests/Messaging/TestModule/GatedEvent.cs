using Ecommerce.Kernel.Application.Messaging;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed record GatedEvent(Guid EntityId, bool Held) : IIntegrationEvent;
