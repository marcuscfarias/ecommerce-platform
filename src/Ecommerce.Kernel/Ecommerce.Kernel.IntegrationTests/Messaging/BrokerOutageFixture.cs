using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class BrokerOutageFixture : BaseIntegrationFixture<MessagingWebApplicationFactory>
{
    public RabbitMqContainerFixture Broker { get; } = new();

    protected override string[] Schemas => ["auth", "catalog", "customers", "orders"];

    public override async Task InitializeAsync()
    {
        await Broker.InitializeAsync();
        await Broker.PauseAsync();
        await base.InitializeAsync();
    }

    protected override MessagingWebApplicationFactory CreateFactory(DatabaseContainerFixture container) =>
        new(container.ConnectionString, Broker.ConnectionString);

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await Broker.DisposeAsync();
    }
}
