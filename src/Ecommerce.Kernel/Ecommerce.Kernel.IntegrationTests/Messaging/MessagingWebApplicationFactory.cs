using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

internal sealed class MessagingWebApplicationFactory(
    DatabaseContainerFixture database,
    IReadOnlyDictionary<string, string?> settings)
    : EcommerceWebApplicationFactory(new MessagingDatabaseConfiguration(database.ConnectionString))
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration(cfg => cfg.AddInMemoryCollection(settings));
    }
}
