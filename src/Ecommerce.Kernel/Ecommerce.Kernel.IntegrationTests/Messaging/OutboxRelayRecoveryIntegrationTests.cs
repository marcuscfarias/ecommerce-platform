using Ecommerce.Kernel.Infrastructure.Messaging.Outbox;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class OutboxRelayRecoveryIntegrationTests(DefaultTimingsRelayFixture fixture)
    : IClassFixture<DefaultTimingsRelayFixture>
{
    private static readonly TimeSpan OutageDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PublishDeadline = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task Relay_RowCommittedAfterOutage_ShouldPublishWithin10Seconds()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var exchange = $"RecoveryTest{Guid.NewGuid():N}";
        var queue = await probe.BindTemporaryQueueAsync(exchange);
        await fixture.Broker.PauseAsync();
        try
        {
            await fixture.InsertOutboxRowsAsync(NewRow(exchange, DateTimeOffset.UtcNow));
            await Task.Delay(OutageDuration);
        }
        finally
        {
            await fixture.Broker.UnpauseAsync();
        }

        var committedAfterOutage = NewRow(exchange, DateTimeOffset.UtcNow);

        // Act
        await fixture.InsertOutboxRowsAsync(committedAfterOutage);
        var received = await probe.ReceiveUntilAsync(
            queue,
            messages => messages.Any(message => message.BasicProperties.MessageId == committedAfterOutage.Id.ToString()),
            PublishDeadline);

        // Assert
        received.ShouldContain(message => message.BasicProperties.MessageId == committedAfterOutage.Id.ToString());
    }

    private static OutboxMessage NewRow(string exchange, DateTimeOffset occurredOnUtc) => new()
    {
        Id = Guid.CreateVersion7(),
        EventName = exchange,
        Payload = "{}",
        OccurredOnUtc = occurredOnUtc,
    };
}
