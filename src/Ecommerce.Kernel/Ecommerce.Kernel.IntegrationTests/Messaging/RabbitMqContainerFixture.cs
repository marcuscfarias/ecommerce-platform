using Testcontainers.RabbitMq;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class RabbitMqContainerFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder("rabbitmq:4-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task PauseAsync() => _container.PauseAsync();

    public Task UnpauseAsync() => _container.UnpauseAsync();

    public async Task<string> ListConnectionsAsync()
    {
        var result = await _container.ExecAsync(
            ["rabbitmqctl", "list_connections", "--quiet", "--no-table-headers", "client_properties"]);

        result.ExitCode.ShouldBe(0, result.Stderr);
        return result.Stdout.Trim();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
