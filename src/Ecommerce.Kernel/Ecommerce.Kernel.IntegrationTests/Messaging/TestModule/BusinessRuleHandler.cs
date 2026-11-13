using Ecommerce.Kernel.Application.Messaging;
using MediatR;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class BusinessRuleHandler(ISender sender) : IIntegrationEventHandler<BusinessRuleEvent>
{
    public const string Module = "alpha";

    public Task HandleAsync(BusinessRuleEvent integrationEvent, CancellationToken cancellationToken) =>
        sender.Send(new FailCommand(integrationEvent.EntityId, BreaksBusinessRule: true), cancellationToken);
}
