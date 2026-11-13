using MediatR;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class RecordHandledCommandHandler(MessagingTestDbContext db) : IRequestHandler<RecordHandledCommand>
{
    public async Task Handle(RecordHandledCommand request, CancellationToken cancellationToken)
    {
        db.HandledMessages.Add(new HandledMessage
        {
            Id = Guid.NewGuid(),
            EntityId = request.EntityId,
            Handler = request.Handler,
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
