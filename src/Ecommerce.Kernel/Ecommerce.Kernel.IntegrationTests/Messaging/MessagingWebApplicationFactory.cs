using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class MessagingWebApplicationFactory(
    DatabaseContainerFixture database,
    IReadOnlyDictionary<string, string?> settings,
    Action<IServiceCollection>? configureTestServices = null)
    : EcommerceWebApplicationFactory(new MessagingDatabaseConfiguration(database.ConnectionString))
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration(cfg => cfg.AddInMemoryCollection(settings));

        if (configureTestServices is not null)
        {
            builder.ConfigureTestServices(configureTestServices);
        }
    }
}
