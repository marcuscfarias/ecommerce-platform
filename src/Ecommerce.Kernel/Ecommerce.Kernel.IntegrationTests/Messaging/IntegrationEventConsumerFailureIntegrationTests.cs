using System.Text;
using System.Text.Json;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class IntegrationEventConsumerFailureIntegrationTests(IntegrationEventConsumerFixture fixture)
    : IClassFixture<IntegrationEventConsumerFixture>
{
    private const int AttemptsWithFiveRetries = 6;
    private const int HeldBelowPrefetch = 9;

    private static readonly TimeSpan ConsumeDeadline = TimeSpan.FromSeconds(15);

    [Fact]
    public async Task Consume_HandlerAlwaysFails_ShouldAttemptSixTimes()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var @event = new AlwaysFailsEvent(Guid.NewGuid());
        var messageId = Guid.CreateVersion7().ToString();

        // Act
        await probe.PublishAsync(nameof(AlwaysFailsEvent), messageId, Serialize(@event));
        await WaitForDeadLetterAsync(probe, IntegrationEventConsumerFixture.AlwaysFailsQueue, messageId);

        // Assert
        fixture.Attempts.Get(@event.EntityId).ShouldBe(AttemptsWithFiveRetries);
    }

    [Fact]
    public async Task Consume_HandlerAlwaysFails_ShouldDeadLetterAfterLastAttempt()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var @event = new AlwaysFailsEvent(Guid.NewGuid());
        var messageId = Guid.CreateVersion7().ToString();

        // Act
        await probe.PublishAsync(nameof(AlwaysFailsEvent), messageId, Serialize(@event));
        var deadLettered = await WaitForDeadLetterAsync(probe, IntegrationEventConsumerFixture.AlwaysFailsQueue, messageId);

        // Assert
        deadLettered.ShouldBeTrue();
        fixture.Attempts.Get(@event.EntityId).ShouldBe(AttemptsWithFiveRetries);
    }

    [Fact]
    public async Task Consume_BusinessRuleViolation_ShouldDeadLetterAfterOneAttempt()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var @event = new BusinessRuleEvent(Guid.NewGuid());
        var messageId = Guid.CreateVersion7().ToString();

        // Act
        await probe.PublishAsync(nameof(BusinessRuleEvent), messageId, Serialize(@event));
        var deadLettered = await WaitForDeadLetterAsync(probe, IntegrationEventConsumerFixture.BusinessRuleQueue, messageId);

        // Assert
        deadLettered.ShouldBeTrue();
        fixture.Attempts.Get(@event.EntityId).ShouldBe(1);
    }

    [Fact]
    public async Task Consume_UndeserializableBody_ShouldDeadLetterWithoutRunningHandler()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var entityId = Guid.NewGuid();
        var truncatedBody = Encoding.UTF8.GetBytes($$"""{"entityId":"{{entityId}}" """);
        var messageId = Guid.CreateVersion7().ToString();

        // Act
        await probe.PublishAsync(nameof(AlwaysFailsEvent), messageId, truncatedBody);
        var deadLettered = await WaitForDeadLetterAsync(probe, IntegrationEventConsumerFixture.AlwaysFailsQueue, messageId);

        // Assert
        deadLettered.ShouldBeTrue();
        fixture.Attempts.Get(entityId).ShouldBe(0);
    }

    [Fact]
    public async Task Consume_MessageInRetry_ShouldKeepProcessingOtherMessages()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var held = new GatedEvent(Guid.NewGuid(), Held: true);
        var other = new GatedEvent(Guid.NewGuid(), Held: false);
        await probe.PublishAsync(nameof(GatedEvent), Guid.CreateVersion7().ToString(), Serialize(held));
        await Poll.UntilAsync(
            () => Task.FromResult(fixture.Attempts.Get(held.EntityId)), attempts => attempts >= 2, ConsumeDeadline);

        try
        {
            // Act
            await probe.PublishAsync(nameof(GatedEvent), Guid.CreateVersion7().ToString(), Serialize(other));
            var handled = await Poll.UntilAsync(
                () => fixture.HandledMessagesForAsync(other.EntityId), rows => rows.Count > 0, ConsumeDeadline);

            // Assert
            fixture.Attempts.Get(held.EntityId).ShouldBe(2);
            handled.ShouldHaveSingleItem();
            (await fixture.HandledMessagesForAsync(held.EntityId)).ShouldBeEmpty();
        }
        finally
        {
            fixture.Gate.Release(held.EntityId);
        }
    }

    [Fact]
    public async Task Consume_FewerThanPrefetchInRetry_ShouldKeepProcessingOtherMessages()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var held = Enumerable.Range(0, HeldBelowPrefetch).Select(_ => new GatedEvent(Guid.NewGuid(), Held: true)).ToList();
        var other = new GatedEvent(Guid.NewGuid(), Held: false);
        foreach (var @event in held)
        {
            await probe.PublishAsync(nameof(GatedEvent), Guid.CreateVersion7().ToString(), Serialize(@event));
        }

        try
        {
            await Poll.UntilAsync(
                () => Task.FromResult(held.Count(@event => fixture.Attempts.Get(@event.EntityId) >= 2)),
                inRetry => inRetry == HeldBelowPrefetch,
                ConsumeDeadline);

            // Act
            await probe.PublishAsync(nameof(GatedEvent), Guid.CreateVersion7().ToString(), Serialize(other));
            var handled = await Poll.UntilAsync(
                () => fixture.HandledMessagesForAsync(other.EntityId), rows => rows.Count > 0, ConsumeDeadline);

            // Assert
            handled.ShouldHaveSingleItem();
            held.ShouldAllBe(@event => fixture.Attempts.Get(@event.EntityId) == 2);
        }
        finally
        {
            held.ForEach(@event => fixture.Gate.Release(@event.EntityId));
        }
    }

    [Fact]
    public async Task Consume_DeadLettered_ShouldLogWarningWithoutPayload()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var @event = new AlwaysFailsEvent(Guid.NewGuid());
        var body = Serialize(@event);
        var messageId = Guid.CreateVersion7().ToString();
        fixture.Logs.Clear();

        // Act
        await probe.PublishAsync(nameof(AlwaysFailsEvent), messageId, body);
        await WaitForDeadLetterAsync(probe, IntegrationEventConsumerFixture.AlwaysFailsQueue, messageId);

        // Assert
        var records = fixture.Logs.GetSnapshot();
        var deadLettered = records
            .Where(record => record.Level == LogLevel.Warning && record.Message.Contains(messageId))
            .ShouldHaveSingleItem();
        deadLettered.GetStructuredStateValue("EventName").ShouldBe(nameof(AlwaysFailsEvent));
        deadLettered.GetStructuredStateValue("Queue").ShouldBe(IntegrationEventConsumerFixture.AlwaysFailsQueue);
        deadLettered.GetStructuredStateValue("Attempts").ShouldBe("6");
        var payload = Encoding.UTF8.GetString(body);
        records.ShouldNotContain(record =>
            record.Message.Contains(@event.EntityId.ToString())
            || record.Message.Contains(payload)
            || (record.Exception != null && record.Exception.ToString().Contains(@event.EntityId.ToString())));
    }

    private static async Task<bool> WaitForDeadLetterAsync(BrokerProbe probe, string queue, string messageId)
    {
        var received = await probe.ReceiveUntilAsync(
            queue + ".dlq",
            messages => messages.Any(message => message.BasicProperties.MessageId == messageId),
            ConsumeDeadline);
        return received.Any(message => message.BasicProperties.MessageId == messageId);
    }

    private static byte[] Serialize<TEvent>(TEvent @event) =>
        JsonSerializer.SerializeToUtf8Bytes(@event, JsonSerializerOptions.Web);
}
