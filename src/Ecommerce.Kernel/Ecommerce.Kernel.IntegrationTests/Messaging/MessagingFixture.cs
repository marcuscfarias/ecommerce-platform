using Ecommerce.Kernel.Infrastructure.Messaging.Inbox;
using Ecommerce.Kernel.Infrastructure.Messaging.Outbox;
using Ecommerce.Kernel.IntegrationTests.Database;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public abstract class MessagingFixture : BaseIntegrationFixture<MessagingWebApplicationFactory>
{
    private static readonly TimeSpan TopologyDeadline = TimeSpan.FromSeconds(30);

    public RabbitMqContainerFixture Broker { get; } = new();

    public FakeLogCollector Logs => Factory.Services.GetFakeLogCollector();

    internal AttemptCounter Attempts => Factory.Services.GetRequiredService<AttemptCounter>();

    internal RetryGate Gate => Factory.Services.GetRequiredService<RetryGate>();

    protected override string[] Schemas => [MessagingTestDbContext.Schema];

    protected virtual string[] ConsumerQueues => [];

    public override async Task InitializeAsync()
    {
        await Broker.InitializeAsync();
        await base.InitializeAsync();

        if (ConsumerQueues.Length == 0)
        {
            return;
        }

        var declared = await Poll.UntilAsync(
            () => Broker.ListQueuesAsync("name"),
            output => ConsumerQueues.All(queue => output.Contains(queue, StringComparison.Ordinal)),
            TopologyDeadline);
        ConsumerQueues.ShouldAllBe(queue => declared.Contains(queue, StringComparison.Ordinal));
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await Broker.DisposeAsync();
    }

    internal async Task WriteEventAsync(RecordedEvent @event)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        db.TestEntities.Add(new TestEntity { Id = @event.EntityId, Name = @event.Name });
        db.AddIntegrationEvent(@event);
        await db.SaveChangesAsync();
    }

    internal async Task WriteEventAndRollBackAsync(RecordedEvent @event)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        db.TestEntities.Add(new TestEntity { Id = @event.EntityId, Name = @event.Name });
        db.AddIntegrationEvent(@event);
        await db.SaveChangesAsync();
        await transaction.RollbackAsync();
    }

    internal Task InsertOutboxRowsAsync(params OutboxMessage[] rows) =>
        SeedAsync<MessagingTestDbContext>(db =>
        {
            db.Set<OutboxMessage>().AddRange(rows);
            return Task.CompletedTask;
        });

    internal Task<bool> OutboxRowExistsAsync(Guid id) =>
        QueryAsync(db => db.Set<OutboxMessage>().AnyAsync(row => row.Id == id));

    internal Task<bool> TestEntityExistsAsync(Guid id) =>
        QueryAsync(db => db.TestEntities.AnyAsync(entity => entity.Id == id));

    internal Task<List<HandledMessage>> HandledMessagesForAsync(Guid entityId) =>
        QueryAsync(db => db.HandledMessages.Where(message => message.EntityId == entityId).ToListAsync());

    internal Task<List<InboxMessage>> InboxMessagesForAsync(Guid messageId) =>
        QueryAsync(db => db.Set<InboxMessage>().Where(message => message.MessageId == messageId).ToListAsync());

    protected abstract void ConfigureTestServices(IServiceCollection services);

    protected sealed override MessagingWebApplicationFactory CreateFactory(DatabaseContainerFixture container) =>
        new(
            container,
            MessagingTestSettings.Enabled(Broker.ConnectionString),
            services =>
            {
                services.AddFakeLogging().AddMessagingTestModule();
                ConfigureTestServices(services);
            });

    private async Task<T> QueryAsync<T>(Func<MessagingTestDbContext, Task<T>> query)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>());
    }
}
