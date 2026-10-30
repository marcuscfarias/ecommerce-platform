using DotNet.Testcontainers.Containers;
using Testcontainers.RabbitMq;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class RabbitMqContainerFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder("rabbitmq:4-alpine").Build();

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public Task PauseAsync() => _container.PauseAsync();

    public Task UnpauseAsync() => _container.UnpauseAsync();

    public async Task DisposeAsync()
    {
        if (_container.State == TestcontainersStates.Paused)
        {
            await _container.UnpauseAsync();
        }

        await _container.DisposeAsync();
    }
}
