using Ecommerce.Kernel.Infrastructure.Messaging;
using Ecommerce.Kernel.IntegrationTests.Database;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class OutboxRelayFixture : BaseIntegrationFixture<MessagingWebApplicationFactory>
{
    public RabbitMqContainerFixture Broker { get; } = new();

    public IServiceProvider Services => Factory.Services;

    protected override string[] Schemas => [MessagingTestDbContext.Schema];

    public override async Task InitializeAsync()
    {
        await Broker.InitializeAsync();
        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await Broker.DisposeAsync();
    }

    protected override MessagingWebApplicationFactory CreateFactory(DatabaseContainerFixture container) =>
        new(
            container,
            MessagingTestSettings.Enabled(Broker.ConnectionString),
            services => services
                .AddMessagingTestModule()
                .AddOutboxRelay<MessagingTestDbContext>());
}
