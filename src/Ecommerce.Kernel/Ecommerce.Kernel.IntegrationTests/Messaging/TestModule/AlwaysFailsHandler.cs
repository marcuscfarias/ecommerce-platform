using Ecommerce.Kernel.Application.Messaging;
using MediatR;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class AlwaysFailsHandler(ISender sender) : IIntegrationEventHandler<AlwaysFailsEvent>
{
    public const string Module = "alpha";

    public Task HandleAsync(AlwaysFailsEvent integrationEvent, CancellationToken cancellationToken) =>
        sender.Send(new FailCommand(integrationEvent.EntityId, BreaksBusinessRule: false), cancellationToken);
}
