using System.Net.Sockets;
using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public class BrokerHealthIntegrationTests(DatabaseContainerFixture database, RabbitMqContainerFixture broker)
    : IClassFixture<DatabaseContainerFixture>, IClassFixture<RabbitMqContainerFixture>
{
    private const string RabbitMqEntry = "rabbitmq";

    private static readonly TimeSpan ConnectDeadline = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan SilentBrokerDeadline = TimeSpan.FromSeconds(60);

    [Fact]
    public async Task Health_BrokerUnreachable_ShouldReportRabbitMqDegradedWith200()
    {
        // Arrange
        await using var factory = CreateFactory(enabled: true, $"amqp://guest:guest@127.0.0.1:{GetUnusedPort()}/");
        var client = factory.CreateClient();

        // Act
        var (statusCode, health) = await GetHealthAsync(client);

        // Assert
        statusCode.ShouldBe(HttpStatusCode.OK);
        health.Entries[RabbitMqEntry].Status.ShouldBe("Degraded");
    }

    [Fact]
    public async Task Health_BrokerReachable_ShouldReportRabbitMqHealthy()
    {
        // Arrange
        await using var factory = CreateFactory(enabled: true, broker.ConnectionString);
        var client = factory.CreateClient();

        // Act
        var (statusCode, health) = await Poll.UntilAsync(
            () => GetHealthAsync(client),
            result => result.Health.Entries[RabbitMqEntry].Status == "Healthy",
            ConnectDeadline);

        // Assert
        statusCode.ShouldBe(HttpStatusCode.OK);
        health.Entries[RabbitMqEntry].Status.ShouldBe("Healthy");
    }

    [Fact]
    public async Task Health_BrokerSilentAfterConnecting_ShouldReportRabbitMqDegraded()
    {
        // Arrange
        await using var factory = CreateFactory(enabled: true, broker.ConnectionString);
        var client = factory.CreateClient();
        await Poll.UntilAsync(
            () => GetHealthAsync(client),
            result => result.Health.Entries[RabbitMqEntry].Status == "Healthy",
            ConnectDeadline);
        await broker.PauseAsync();

        try
        {
            // Act
            var (statusCode, health) = await Poll.UntilAsync(
                () => GetHealthAsync(client),
                result => result.Health.Entries[RabbitMqEntry].Status == "Degraded",
                SilentBrokerDeadline);

            // Assert
            statusCode.ShouldBe(HttpStatusCode.OK);
            health.Entries[RabbitMqEntry].Status.ShouldBe("Degraded");
        }
        finally
        {
            await broker.UnpauseAsync();
        }
    }

    [Fact]
    public async Task Health_MessagingDisabled_ShouldReportRabbitMqHealthy()
    {
        // Arrange
        await using var factory = CreateFactory(enabled: false, string.Empty);
        var client = factory.CreateClient();

        // Act
        var (statusCode, health) = await GetHealthAsync(client);

        // Assert
        statusCode.ShouldBe(HttpStatusCode.OK);
        health.Entries[RabbitMqEntry].Status.ShouldBe("Healthy");
    }

    private MessagingWebApplicationFactory CreateFactory(bool enabled, string connectionString) =>
        new(database, new Dictionary<string, string?>
        {
            ["Messaging:Enabled"] = enabled ? "true" : "false",
            ["ConnectionStrings:RabbitMq"] = connectionString,
        });

    private static async Task<(HttpStatusCode StatusCode, HealthResponse Health)> GetHealthAsync(HttpClient client)
    {
        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative));
        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
        return (response.StatusCode, health!);
    }

    private static int GetUnusedPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private sealed record HealthResponse(string Status, Dictionary<string, HealthEntryResponse> Entries);

    private sealed record HealthEntryResponse(string Status, string? Description);
}
