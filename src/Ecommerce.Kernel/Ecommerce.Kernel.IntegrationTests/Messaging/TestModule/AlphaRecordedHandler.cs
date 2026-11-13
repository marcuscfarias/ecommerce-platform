using Ecommerce.Kernel.Application.Messaging;
using MediatR;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class AlphaRecordedHandler(ISender sender) : IIntegrationEventHandler<RecordedEvent>
{
    public const string Module = "alpha";

    public Task HandleAsync(RecordedEvent integrationEvent, CancellationToken cancellationToken) =>
        sender.Send(new RecordHandledCommand(integrationEvent.EntityId, Module), cancellationToken);
}
