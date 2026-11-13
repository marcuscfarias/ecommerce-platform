using System.Text.Json;
using Ecommerce.Kernel.Infrastructure.Messaging.Outbox;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class OutboxRelayIntegrationTests(OutboxRelayFixture fixture) : IClassFixture<OutboxRelayFixture>
{
    private const string RecordedEventExchange = "RecordedEvent";
    private const string EmptyPayload = "{}";

    private static readonly TimeSpan PublishDeadline = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task Relay_CommittedRow_ShouldPublishWithin10Seconds()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var queue = await probe.BindTemporaryQueueAsync(RecordedEventExchange);
        var @event = new RecordedEvent(Guid.NewGuid(), "committed");

        // Act
        await WriteEventAsync(@event);
        var received = await probe.ReceiveUntilAsync(
            queue, messages => messages.Any(message => Carries(message, @event)), PublishDeadline);

        // Assert
        received.ShouldContain(message => Carries(message, @event));
    }

    [Fact]
    public async Task Relay_ConfirmedPublish_ShouldDeleteRow()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var row = NewRow(UniqueExchange(), DateTimeOffset.UtcNow);
        var queue = await probe.BindTemporaryQueueAsync(row.EventName);

        // Act
        await InsertRowsAsync(row);
        await probe.WaitForMessagesAsync(queue, 1, PublishDeadline);
        var rowExists = await Poll.UntilAsync(() => RowExistsAsync(row.Id), exists => !exists, PublishDeadline);

        // Assert
        rowExists.ShouldBeFalse();
    }

    [Fact]
    public async Task Relay_PendingRows_ShouldPublishOldestFirst()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var exchange = UniqueExchange();
        var queue = await probe.BindTemporaryQueueAsync(exchange);
        var now = DateTimeOffset.UtcNow;
        var oldest = NewRow(exchange, now.AddSeconds(-3));
        var middle = NewRow(exchange, now.AddSeconds(-2));
        var newest = NewRow(exchange, now.AddSeconds(-1));

        // Act
        await InsertRowsAsync(newest, oldest, middle);
        var received = await probe.WaitForMessagesAsync(queue, 3, PublishDeadline);

        // Assert
        received.Select(message => message.BasicProperties.MessageId)
            .ShouldBe([oldest.Id.ToString(), middle.Id.ToString(), newest.Id.ToString()]);
    }

    [Fact]
    public async Task Relay_PublishedMessage_ShouldCarryRowIdAsMessageId()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var row = NewRow(UniqueExchange(), DateTimeOffset.UtcNow);
        var queue = await probe.BindTemporaryQueueAsync(row.EventName);

        // Act
        await InsertRowsAsync(row);
        var received = await probe.WaitForMessagesAsync(queue, 1, PublishDeadline);

        // Assert
        received.ShouldHaveSingleItem().BasicProperties.MessageId.ShouldBe(row.Id.ToString());
    }

    [Fact]
    public async Task Relay_RolledBackWrite_ShouldPublishNothing()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var queue = await probe.BindTemporaryQueueAsync(RecordedEventExchange);
        var rolledBack = new RecordedEvent(Guid.NewGuid(), "rolled-back");
        var marker = new RecordedEvent(Guid.NewGuid(), "committed-after");

        // Act
        await WriteEventAndRollBackAsync(rolledBack);
        await WriteEventAsync(marker);
        var received = await probe.ReceiveUntilAsync(
            queue, messages => messages.Any(message => Carries(message, marker)), PublishDeadline);

        // Assert
        received.ShouldContain(message => Carries(message, marker));
        received.ShouldNotContain(message => Carries(message, rolledBack));
    }

    [Fact]
    public async Task Relay_ExchangeMissing_ShouldDeclareAndPublish()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var row = NewRow(UniqueExchange(), DateTimeOffset.UtcNow);
        (await probe.ExchangeExistsAsync(row.EventName)).ShouldBeFalse();

        // Act
        await InsertRowsAsync(row);
        var rowExists = await Poll.UntilAsync(() => RowExistsAsync(row.Id), exists => !exists, PublishDeadline);

        // Assert
        rowExists.ShouldBeFalse();
        (await probe.ExchangeExistsAsync(row.EventName)).ShouldBeTrue();
    }

    private async Task WriteEventAsync(RecordedEvent @event)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        db.TestEntities.Add(new TestEntity { Id = @event.EntityId, Name = @event.Name });
        db.AddIntegrationEvent(@event);
        await db.SaveChangesAsync();
    }

    private async Task WriteEventAndRollBackAsync(RecordedEvent @event)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        db.TestEntities.Add(new TestEntity { Id = @event.EntityId, Name = @event.Name });
        db.AddIntegrationEvent(@event);
        await db.SaveChangesAsync();
        await transaction.RollbackAsync();
    }

    private Task InsertRowsAsync(params OutboxMessage[] rows) =>
        fixture.SeedAsync<MessagingTestDbContext>(db =>
        {
            db.Set<OutboxMessage>().AddRange(rows);
            return Task.CompletedTask;
        });

    private async Task<bool> RowExistsAsync(Guid id)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        return await db.Set<OutboxMessage>().AnyAsync(row => row.Id == id);
    }

    private static OutboxMessage NewRow(string exchange, DateTimeOffset occurredOnUtc) => new()
    {
        Id = Guid.CreateVersion7(),
        EventName = exchange,
        Payload = EmptyPayload,
        OccurredOnUtc = occurredOnUtc,
    };

    private static string UniqueExchange() => $"RelayTest{Guid.NewGuid():N}";

    private static bool Carries(BasicGetResult message, RecordedEvent @event) =>
        JsonSerializer.Deserialize<RecordedEvent>(message.Body.Span, JsonSerializerOptions.Web) == @event;
}
