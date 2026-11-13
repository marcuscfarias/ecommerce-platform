using System.Net.Sockets;
using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class BrokerConnectionIntegrationTests(DatabaseContainerFixture database, RabbitMqContainerFixture broker)
    : IClassFixture<DatabaseContainerFixture>, IClassFixture<RabbitMqContainerFixture>
{
    private const string ClientName = "ecommerce-api";

    private static readonly TimeSpan HealthDeadline = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan ConnectDeadline = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan NoConnectionWindow = TimeSpan.FromSeconds(2);

    [Fact]
    public async Task Boot_BrokerPortRefused_ShouldAnswerHealthWithin10Seconds()
    {
        // Arrange
        await using var factory = CreateFactory(enabled: true, $"amqp://guest:guest@127.0.0.1:{GetUnusedPort()}/");

        // Act
        var response = await BootAndGetHealthAsync(factory);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Boot_BrokerPaused_ShouldAnswerHealthWithin10Seconds()
    {
        // Arrange
        await using var factory = CreateFactory(enabled: true, broker.ConnectionString);
        await broker.PauseAsync();

        try
        {
            // Act
            var response = await BootAndGetHealthAsync(factory);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
        finally
        {
            await broker.UnpauseAsync();
        }
    }

    [Fact]
    public async Task Boot_MessagingDisabled_ShouldOpenNoBrokerConnection()
    {
        // Arrange
        await using var factory = CreateFactory(enabled: false, broker.ConnectionString);

        // Act
        await BootAndGetHealthAsync(factory);
        await Task.Delay(NoConnectionWindow);

        // Assert
        var connections = await broker.ListConnectionsAsync();
        connections.ShouldBeEmpty();
    }

    [Fact]
    public async Task BrokerConnection_BrokerUnpausedAfterBoot_ShouldConnect()
    {
        // Arrange
        await using var factory = CreateFactory(enabled: true, broker.ConnectionString);
        await broker.PauseAsync();
        try
        {
            await BootAndGetHealthAsync(factory);
        }
        finally
        {
            await broker.UnpauseAsync();
        }

        // Act
        var connections = await Poll.UntilAsync(
            broker.ListConnectionsAsync,
            output => output.Contains(ClientName, StringComparison.Ordinal),
            ConnectDeadline);

        // Assert
        connections.ShouldContain(ClientName);
    }

    [Fact]
    public async Task BrokerConnection_StoppedTwiceAfterConnecting_ShouldNotThrow()
    {
        // Arrange
        await using var factory = CreateFactory(enabled: true, broker.ConnectionString);
        await BootAndGetHealthAsync(factory);
        await Poll.UntilAsync(
            broker.ListConnectionsAsync,
            output => output.Contains(ClientName, StringComparison.Ordinal),
            ConnectDeadline);
        var connectionService = factory.Services.GetServices<IHostedService>()
            .Single(service => service.GetType().Name == "BrokerConnectionService");
        await connectionService.StopAsync(CancellationToken.None);

        // Act
        var act = () => connectionService.StopAsync(CancellationToken.None);

        // Assert
        await Should.NotThrowAsync(act);
    }

    private MessagingWebApplicationFactory CreateFactory(bool enabled, string connectionString) =>
        new(database, new Dictionary<string, string?>
        {
            ["Messaging:Enabled"] = enabled ? "true" : "false",
            ["ConnectionStrings:RabbitMq"] = connectionString,
        });

    private static Task<HttpResponseMessage> BootAndGetHealthAsync(MessagingWebApplicationFactory factory) =>
        Task.Run(() => factory.CreateClient().GetAsync(new Uri("/health", UriKind.Relative)))
            .WaitAsync(HealthDeadline);

    private static int GetUnusedPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
