using System.Text.Json;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class IntegrationEventConsumerOutageIntegrationTests(IntegrationEventConsumerFixture fixture)
    : IClassFixture<IntegrationEventConsumerFixture>
{
    private static readonly TimeSpan OutageDuration = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan RecoveryDeadline = TimeSpan.FromSeconds(60);

    [Fact]
    public async Task Consume_BrokerPausedThenResumed_ShouldResumeConsuming()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var @event = new RecordedEvent(Guid.NewGuid(), "after-outage");
        await fixture.Broker.PauseAsync();
        try
        {
            await Task.Delay(OutageDuration);
        }
        finally
        {
            await fixture.Broker.UnpauseAsync();
        }

        await fixture.Broker.CloseAllConnectionsAsync();

        // Act
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        await probe.PublishAsync(nameof(RecordedEvent), Guid.CreateVersion7().ToString(), Serialize(@event));
        var handled = await Poll.UntilAsync(
            () => fixture.HandledMessagesForAsync(@event.EntityId),
            rows => rows.Any(row => row.Handler == AlphaRecordedHandler.Module),
            RecoveryDeadline);

        // Assert
        handled.ShouldContain(row => row.Handler == AlphaRecordedHandler.Module);
    }

    private static byte[] Serialize<TEvent>(TEvent @event) =>
        JsonSerializer.SerializeToUtf8Bytes(@event, JsonSerializerOptions.Web);
}
