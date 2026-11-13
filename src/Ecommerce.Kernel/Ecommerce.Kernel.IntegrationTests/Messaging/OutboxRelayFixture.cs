using Ecommerce.Kernel.Infrastructure.Messaging;
using Ecommerce.Kernel.Infrastructure.Messaging.Outbox;
using Ecommerce.Kernel.IntegrationTests.Database;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class OutboxRelayFixture : BaseIntegrationFixture<MessagingWebApplicationFactory>
{
    public RabbitMqContainerFixture Broker { get; } = new();

    public FakeLogCollector Logs => Factory.Services.GetFakeLogCollector();

    protected override string[] Schemas => [MessagingTestDbContext.Schema];

    public override async Task InitializeAsync()
    {
        await Broker.InitializeAsync();
        await base.InitializeAsync();
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

    internal async Task<bool> OutboxRowExistsAsync(Guid id)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        return await db.Set<OutboxMessage>().AnyAsync(row => row.Id == id);
    }

    internal async Task<bool> TestEntityExistsAsync(Guid id)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        return await db.TestEntities.AnyAsync(entity => entity.Id == id);
    }

    protected override MessagingWebApplicationFactory CreateFactory(DatabaseContainerFixture container) =>
        new(
            container,
            MessagingTestSettings.Enabled(Broker.ConnectionString),
            services => services
                .AddFakeLogging()
                .AddMessagingTestModule()
                .AddOutboxRelay<MessagingTestDbContext>());
}
