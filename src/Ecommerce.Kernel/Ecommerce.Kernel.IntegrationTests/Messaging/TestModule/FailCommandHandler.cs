using Ecommerce.Kernel.Domain.Exceptions;
using MediatR;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class FailCommandHandler(AttemptCounter counter) : IRequestHandler<FailCommand>
{
    public Task Handle(FailCommand request, CancellationToken cancellationToken)
    {
        counter.Increment(request.EntityId);

        if (request.BreaksBusinessRule)
        {
            throw new BusinessRuleValidationException("The event breaks a business rule.");
        }

        throw new InvalidOperationException("The handler always fails.");
    }
}
