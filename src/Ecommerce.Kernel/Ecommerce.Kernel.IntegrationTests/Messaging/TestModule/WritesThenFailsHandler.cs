using Ecommerce.Kernel.Application.Messaging;
using MediatR;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class WritesThenFailsHandler(ISender sender) : IIntegrationEventHandler<WritesThenFailsEvent>
{
    public const string Module = "alpha";

    public async Task HandleAsync(WritesThenFailsEvent integrationEvent, CancellationToken cancellationToken)
    {
        await sender.Send(new RecordHandledCommand(integrationEvent.EntityId, Module), cancellationToken);
        throw new InvalidOperationException("The handler fails after writing.");
    }
}
