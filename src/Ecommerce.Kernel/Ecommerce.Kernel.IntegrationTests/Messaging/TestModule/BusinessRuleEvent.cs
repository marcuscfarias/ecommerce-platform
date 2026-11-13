using Ecommerce.Kernel.Application.Messaging;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed record BusinessRuleEvent(Guid EntityId) : IIntegrationEvent;
