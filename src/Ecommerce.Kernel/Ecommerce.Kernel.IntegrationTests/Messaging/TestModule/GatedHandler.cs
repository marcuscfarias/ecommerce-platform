using Ecommerce.Kernel.Application.Messaging;
using MediatR;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class GatedHandler(ISender sender) : IIntegrationEventHandler<GatedEvent>
{
    public const string Module = "alpha";

    public Task HandleAsync(GatedEvent integrationEvent, CancellationToken cancellationToken) =>
        sender.Send(new GatedCommand(integrationEvent.EntityId, integrationEvent.Held, Module), cancellationToken);
}
