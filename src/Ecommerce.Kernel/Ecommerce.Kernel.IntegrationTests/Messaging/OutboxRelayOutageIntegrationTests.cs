using Ecommerce.Kernel.Infrastructure.Messaging.Outbox;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class OutboxRelayOutageIntegrationTests(OutboxRelayFixture fixture) : IClassFixture<OutboxRelayFixture>
{
    private static readonly TimeSpan WriteDeadline = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan FailedPublishWindow = TimeSpan.FromSeconds(4);
    private static readonly TimeSpan FailedPublishDeadline = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan RecoveryDeadline = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task OutboxWrite_BrokerPaused_ShouldSucceed()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var @event = new RecordedEvent(Guid.NewGuid(), "written-during-outage");
        await fixture.Broker.PauseAsync();

        try
        {
            // Act
            await fixture.WriteEventAsync(@event).WaitAsync(WriteDeadline);

            // Assert
            (await fixture.TestEntityExistsAsync(@event.EntityId)).ShouldBeTrue();
        }
        finally
        {
            await fixture.Broker.UnpauseAsync();
        }
    }

    [Fact]
    public async Task Relay_BrokerPaused_ShouldKeepRowAndPublishAfterUnpause()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var row = NewRow("{}");
        var queue = await probe.BindTemporaryQueueAsync(row.EventName);
        await fixture.Broker.PauseAsync();
        bool rowKeptDuringOutage;
        try
        {
            await fixture.InsertOutboxRowsAsync(row);
            await Task.Delay(FailedPublishWindow);
            rowKeptDuringOutage = await fixture.OutboxRowExistsAsync(row.Id);
        }
        finally
        {
            await fixture.Broker.UnpauseAsync();
        }

        // Act
        var received = await probe.ReceiveUntilAsync(
            queue,
            messages => messages.Any(message => message.BasicProperties.MessageId == row.Id.ToString()),
            RecoveryDeadline);

        // Assert
        rowKeptDuringOutage.ShouldBeTrue();
        received.ShouldContain(message => message.BasicProperties.MessageId == row.Id.ToString());
    }

    [Fact]
    public async Task Relay_PublishFails_ShouldLogWarningWithoutPayload()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        var secret = Guid.NewGuid().ToString("N");
        var row = NewRow($$"""{"secret":"{{secret}}"}""");
        fixture.Logs.Clear();
        await fixture.Broker.PauseAsync();

        try
        {
            // Act
            await fixture.InsertOutboxRowsAsync(row);
            var warnings = await Poll.UntilAsync(
                () => Task.FromResult(PublishWarningsFor(row)),
                records => records.Count > 0,
                FailedPublishDeadline);

            // Assert
            warnings.ShouldNotBeEmpty();
            warnings.ShouldAllBe(record => record.Message.Contains(row.EventName));
            fixture.Logs.GetSnapshot().ShouldNotContain(record =>
                record.Message.Contains(secret) || (record.Exception != null && record.Exception.ToString().Contains(secret)));
        }
        finally
        {
            await fixture.Broker.UnpauseAsync();
        }
    }

    [Fact]
    public async Task Relay_ConnectionDropped_ShouldPublishAfterRecovery()
    {
        await fixture.ResetDatabaseAsync();

        // Arrange
        await fixture.Broker.CloseAllConnectionsAsync();
        await using var probe = await BrokerProbe.ConnectAsync(fixture.Broker.ConnectionString);
        var row = NewRow("{}");
        var queue = await probe.BindTemporaryQueueAsync(row.EventName);

        // Act
        await fixture.InsertOutboxRowsAsync(row);
        var received = await probe.ReceiveUntilAsync(
            queue,
            messages => messages.Any(message => message.BasicProperties.MessageId == row.Id.ToString()),
            RecoveryDeadline);

        // Assert
        received.ShouldContain(message => message.BasicProperties.MessageId == row.Id.ToString());
    }

    private List<FakeLogRecord> PublishWarningsFor(OutboxMessage row) =>
        fixture.Logs.GetSnapshot()
            .Where(record => record.Level == LogLevel.Warning && record.Message.Contains(row.Id.ToString()))
            .ToList();

    private static OutboxMessage NewRow(string payload) => new()
    {
        Id = Guid.CreateVersion7(),
        EventName = $"OutageTest{Guid.NewGuid():N}",
        Payload = payload,
        OccurredOnUtc = DateTimeOffset.UtcNow,
    };
}
