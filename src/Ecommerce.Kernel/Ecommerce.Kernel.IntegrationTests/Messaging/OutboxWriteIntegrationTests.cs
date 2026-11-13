using System.Text.Json;
using Ecommerce.Kernel.Infrastructure.Messaging.Outbox;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class OutboxWriteIntegrationTests(OutboxWriteFixture fixture) : IClassFixture<OutboxWriteFixture>
{
    [Fact]
    public async Task SaveChanges_EntityAndEvent_ShouldPersistOutboxRowInSameTransaction()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "written-with-event" };
        var @event = new RecordedEvent(entity.Id, entity.Name);

        // Act
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
            db.TestEntities.Add(entity);
            db.AddIntegrationEvent(@event);
            await db.SaveChangesAsync();
        }

        // Assert
        await using var verifyScope = fixture.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        (await verifyDb.TestEntities.SingleAsync()).Id.ShouldBe(entity.Id);
        var row = await verifyDb.Set<OutboxMessage>().SingleAsync();
        row.EventName.ShouldBe("RecordedEvent");
        JsonSerializer.Deserialize<RecordedEvent>(row.Payload, JsonSerializerOptions.Web).ShouldBe(@event);
    }

    [Fact]
    public async Task SaveChanges_TransactionRolledBack_ShouldKeepNoOutboxRow()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "rolled-back" };

        // Act
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
            await using var transaction = await db.Database.BeginTransactionAsync();
            db.TestEntities.Add(entity);
            db.AddIntegrationEvent(new RecordedEvent(entity.Id, entity.Name));
            await db.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        // Assert
        await using var verifyScope = fixture.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        (await verifyDb.Set<OutboxMessage>().CountAsync()).ShouldBe(0);
        (await verifyDb.TestEntities.CountAsync()).ShouldBe(0);
    }
}
