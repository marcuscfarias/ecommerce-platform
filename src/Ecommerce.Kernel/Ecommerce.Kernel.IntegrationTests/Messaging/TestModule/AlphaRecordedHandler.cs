using Ecommerce.Kernel.Application.Messaging;
using MediatR;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class AlphaRecordedHandler(ISender sender, AttemptCounter runs) : IIntegrationEventHandler<RecordedEvent>
{
    public const string Module = "alpha";

    public Task HandleAsync(RecordedEvent integrationEvent, CancellationToken cancellationToken)
    {
        runs.Increment(integrationEvent.EntityId);
        return sender.Send(new RecordHandledCommand(integrationEvent.EntityId, Module), cancellationToken);
    }
}
