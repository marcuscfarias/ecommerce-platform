using System.Text.Json;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class IntegrationEventConsumerShutdownIntegrationTests(IntegrationEventConsumerFixture fixture)
    : IClassFixture<IntegrationEventConsumerFixture>
{
    private static readonly TimeSpan ConsumeDeadline = TimeSpan.FromSeconds(15);

    [Fact]
    public async Task Consume_HostStopsMidHandling_ShouldLeaveMessageForRedelivery()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var held = new GatedEvent(Guid.NewGuid(), Held: true);
        var messageId = Guid.CreateVersion7().ToString();
        await probe.PublishAsync(
            nameof(GatedEvent), messageId, JsonSerializer.SerializeToUtf8Bytes(held, JsonSerializerOptions.Web));
        await Poll.UntilAsync(
            () => Task.FromResult(fixture.Attempts.Get(held.EntityId)), attempts => attempts >= 2, ConsumeDeadline);

        // Act
        fixture.StopHost();
        var requeued = await probe.ReceiveUntilAsync(
            IntegrationEventConsumerFixture.GatedQueue,
            messages => messages.Any(message => message.BasicProperties.MessageId == messageId),
            ConsumeDeadline);

        // Assert
        var redelivery = requeued.ShouldHaveSingleItem();
        redelivery.BasicProperties.MessageId.ShouldBe(messageId);
        redelivery.Redelivered.ShouldBeTrue();
        (await fixture.Broker.CountMessagesAsync(IntegrationEventConsumerFixture.GatedQueue + ".dlq")).ShouldBe(0);
    }
}
