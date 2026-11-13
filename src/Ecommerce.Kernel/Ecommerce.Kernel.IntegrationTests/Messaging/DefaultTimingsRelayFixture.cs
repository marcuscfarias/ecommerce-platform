using Ecommerce.Kernel.Infrastructure.Messaging;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class DefaultTimingsRelayFixture : MessagingFixture
{
    protected override Dictionary<string, string?> Settings => new()
    {
        ["Messaging:Enabled"] = "true",
        ["ConnectionStrings:RabbitMq"] = Broker.ConnectionString,
    };

    protected override void ConfigureTestServices(IServiceCollection services) =>
        services.AddOutboxRelay<MessagingTestDbContext>();
}
