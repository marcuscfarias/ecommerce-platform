using MediatR;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class GatedCommandHandler(MessagingTestDbContext db, AttemptCounter counter, RetryGate gate)
    : IRequestHandler<GatedCommand>
{
    public async Task Handle(GatedCommand request, CancellationToken cancellationToken)
    {
        var attempt = counter.Increment(request.EntityId);

        if (request.Held)
        {
            if (attempt == 1)
            {
                throw new InvalidOperationException("The held message fails its first attempt.");
            }

            await gate.WaitAsync(request.EntityId, cancellationToken);
        }

        db.HandledMessages.Add(new HandledMessage
        {
            Id = Guid.NewGuid(),
            EntityId = request.EntityId,
            Handler = request.Handler,
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
