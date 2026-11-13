using Ecommerce.Kernel.Infrastructure.Messaging;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class OutboxRelayFixture : MessagingFixture
{
    protected override void ConfigureTestServices(IServiceCollection services) =>
        services.AddOutboxRelay<MessagingTestDbContext>();
}
