using System.Text.Json;
using Ecommerce.Kernel.Infrastructure.Messaging.Inbox;
using Ecommerce.Kernel.Infrastructure.Messaging.Outbox;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class InboxDeduplicationIntegrationTests(IntegrationEventConsumerFixture fixture)
    : IClassFixture<IntegrationEventConsumerFixture>
{
    private const string Queue = IntegrationEventConsumerFixture.AlphaRecordedQueue;

    private static readonly TimeSpan ConsumeDeadline = TimeSpan.FromSeconds(15);

    [Fact]
    public async Task Consume_SameMessageIdTwice_ShouldApplyEffectOnce()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var @event = new RecordedEvent(Guid.NewGuid(), "duplicated");
        var messageId = Guid.CreateVersion7();
        var body = JsonSerializer.SerializeToUtf8Bytes(@event, JsonSerializerOptions.Web);

        // Act
        await probe.PublishAsync(nameof(RecordedEvent), messageId.ToString(), body);
        await probe.PublishAsync(nameof(RecordedEvent), messageId.ToString(), body);
        var queued = await WaitForEmptyQueueAsync();

        // Assert
        queued.ShouldBe(0);
        await AssertAppliedOnceAsync(@event.EntityId, messageId);
    }

    [Fact]
    public async Task Consume_RelayRepublishesRow_ShouldApplyEffectOnce()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var @event = new RecordedEvent(Guid.NewGuid(), "republished");
        var row = new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            EventName = nameof(RecordedEvent),
            Payload = JsonSerializer.Serialize(@event, JsonSerializerOptions.Web),
            OccurredOnUtc = DateTimeOffset.UtcNow,
        };
        await fixture.InsertOutboxRowsAsync(row);
        await Poll.UntilAsync(
            () => fixture.HandledMessagesForAsync(@event.EntityId),
            rows => rows.Any(message => message.Handler == AlphaRecordedHandler.Module),
            ConsumeDeadline);

        // Act
        await fixture.InsertOutboxRowsAsync(row);
        await Poll.UntilAsync(() => fixture.OutboxRowExistsAsync(row.Id), exists => !exists, ConsumeDeadline);
        var queued = await WaitForEmptyQueueAsync();

        // Assert
        queued.ShouldBe(0);
        await AssertAppliedOnceAsync(@event.EntityId, row.Id);
    }

    [Fact]
    public async Task Consume_MessageProcessedByOtherConsumer_ShouldStillRunHandler()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var @event = new RecordedEvent(Guid.NewGuid(), "seen-by-alpha-only");
        var messageId = Guid.CreateVersion7();
        await fixture.InsertInboxRowsAsync(new InboxMessage
        {
            MessageId = messageId,
            Consumer = Queue,
            ProcessedOnUtc = DateTimeOffset.UtcNow,
        });

        // Act
        await probe.PublishAsync(
            nameof(RecordedEvent), messageId.ToString(), JsonSerializer.SerializeToUtf8Bytes(@event, JsonSerializerOptions.Web));
        var handled = await Poll.UntilAsync(
            () => fixture.HandledMessagesForAsync(@event.EntityId),
            rows => rows.Any(row => row.Handler == BetaRecordedHandler.Module),
            ConsumeDeadline);
        await WaitForEmptyQueueAsync();

        // Assert
        handled.Where(row => row.Handler == BetaRecordedHandler.Module).ShouldHaveSingleItem();
        (await fixture.HandledMessagesForAsync(@event.EntityId))
            .ShouldNotContain(row => row.Handler == AlphaRecordedHandler.Module);
        fixture.Attempts.Get(@event.EntityId).ShouldBe(0);
    }

    private Task<int> WaitForEmptyQueueAsync() =>
        Poll.UntilAsync(() => fixture.Broker.CountMessagesAsync(Queue), count => count == 0, ConsumeDeadline);

    private async Task AssertAppliedOnceAsync(Guid entityId, Guid messageId)
    {
        fixture.Attempts.Get(entityId).ShouldBe(1);
        var handled = await fixture.HandledMessagesForAsync(entityId);
        handled.Where(message => message.Handler == AlphaRecordedHandler.Module).ShouldHaveSingleItem();
        var inbox = await fixture.InboxMessagesForAsync(messageId);
        inbox.Where(message => message.Consumer == Queue).ShouldHaveSingleItem();
        (await fixture.Broker.CountMessagesAsync(Queue + ".dlq")).ShouldBe(0);
    }
}
