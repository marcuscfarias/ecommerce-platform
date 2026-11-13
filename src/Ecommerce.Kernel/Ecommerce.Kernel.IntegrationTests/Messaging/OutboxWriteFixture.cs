using Ecommerce.Kernel.IntegrationTests.Database;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class OutboxWriteFixture : BaseIntegrationFixture<MessagingWebApplicationFactory>
{
    public IServiceProvider Services => Factory.Services;

    protected override string[] Schemas => [MessagingTestDbContext.Schema];

    protected override MessagingWebApplicationFactory CreateFactory(DatabaseContainerFixture container) =>
        new(
            container,
            new Dictionary<string, string?> { ["Messaging:Enabled"] = "false" },
            services => services.AddMessagingTestModule());
}
