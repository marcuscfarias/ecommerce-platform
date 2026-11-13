using System.Text.Json;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class IntegrationEventConsumerIntegrationTests(IntegrationEventConsumerFixture fixture)
    : IClassFixture<IntegrationEventConsumerFixture>
{
    private static readonly TimeSpan ConsumeDeadline = TimeSpan.FromSeconds(15);

    [Fact]
    public async Task Consume_PublishedEvent_ShouldRunHandlerRecordInboxAndAck()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var messageId = Guid.CreateVersion7();
        var @event = new RecordedEvent(Guid.NewGuid(), "consumed");

        // Act
        await probe.PublishAsync(nameof(RecordedEvent), messageId.ToString(), Serialize(@event));
        var inbox = await Poll.UntilAsync(
            () => fixture.InboxMessagesForAsync(messageId),
            rows => rows.Any(row => row.Consumer == IntegrationEventConsumerFixture.AlphaRecordedQueue),
            ConsumeDeadline);
        var queuedMessages = await Poll.UntilAsync(
            () => fixture.Broker.CountMessagesAsync(IntegrationEventConsumerFixture.AlphaRecordedQueue),
            count => count == 0,
            ConsumeDeadline);

        // Assert
        inbox.ShouldContain(row => row.Consumer == IntegrationEventConsumerFixture.AlphaRecordedQueue);
        var handled = await fixture.HandledMessagesForAsync(@event.EntityId);
        handled.Where(message => message.Handler == AlphaRecordedHandler.Module).ShouldHaveSingleItem();
        queuedMessages.ShouldBe(0);
    }

    [Fact]
    public async Task Consume_EventWithTwoSubscribers_ShouldRunBothHandlers()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var @event = new RecordedEvent(Guid.NewGuid(), "fanned-out");

        // Act
        await probe.PublishAsync(nameof(RecordedEvent), Guid.CreateVersion7().ToString(), Serialize(@event));
        var handled = await Poll.UntilAsync(
            () => fixture.HandledMessagesForAsync(@event.EntityId),
            rows => rows.Count >= 2,
            ConsumeDeadline);

        // Assert
        handled.Select(message => message.Handler).Order()
            .ShouldBe([AlphaRecordedHandler.Module, BetaRecordedHandler.Module]);
    }

    [Fact]
    public async Task Consume_HandlerThrows_ShouldRollBackEffectAndInbox()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var messageId = Guid.CreateVersion7().ToString();
        var @event = new WritesThenFailsEvent(Guid.NewGuid());

        // Act
        await probe.PublishAsync(nameof(WritesThenFailsEvent), messageId, Serialize(@event));
        var deadLettered = await probe.ReceiveUntilAsync(
            IntegrationEventConsumerFixture.WritesThenFailsQueue + ".dlq",
            messages => messages.Any(message => message.BasicProperties.MessageId == messageId),
            ConsumeDeadline);

        // Assert
        deadLettered.ShouldContain(message => message.BasicProperties.MessageId == messageId);
        (await fixture.HandledMessagesForAsync(@event.EntityId)).ShouldBeEmpty();
        (await fixture.InboxMessagesForAsync(Guid.Parse(messageId))).ShouldBeEmpty();
    }

    [Fact]
    public async Task Consume_FirstSubscription_ShouldDeclareQueueAndDeadLetterQueue()
    {
        // Arrange
        const string queue = IntegrationEventConsumerFixture.AlphaRecordedQueue;

        // Act
        var queues = await fixture.Broker.ListQueuesAsJsonAsync("name", "durable", "arguments");

        // Assert
        var mainQueue = FindQueue(queues, queue);
        mainQueue.GetProperty("durable").GetBoolean().ShouldBeTrue();
        var arguments = ReadArguments(mainQueue);
        arguments["x-dead-letter-exchange"].ShouldBe(string.Empty);
        arguments["x-dead-letter-routing-key"].ShouldBe($"{queue}.dlq");
        FindQueue(queues, $"{queue}.dlq").GetProperty("durable").GetBoolean().ShouldBeTrue();
    }

    private static JsonElement FindQueue(JsonElement queues, string name) =>
        queues.EnumerateArray().Single(queue => queue.GetProperty("name").GetString() == name);

    private static Dictionary<string, string?> ReadArguments(JsonElement queue) =>
        queue.GetProperty("arguments").EnumerateArray()
            .ToDictionary(argument => argument[0].GetString()!, argument => argument[2].GetString());

    private static byte[] Serialize<TEvent>(TEvent @event) =>
        JsonSerializer.SerializeToUtf8Bytes(@event, JsonSerializerOptions.Web);
}
