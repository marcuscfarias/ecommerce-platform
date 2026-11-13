using Ecommerce.Kernel.Application.Messaging;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed record RecordedEvent(Guid EntityId, string Name) : IIntegrationEvent;
