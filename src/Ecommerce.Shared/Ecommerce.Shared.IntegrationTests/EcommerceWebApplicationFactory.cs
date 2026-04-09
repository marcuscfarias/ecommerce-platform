using Ecommerce.AppHost;
using Ecommerce.Shared.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Shared.IntegrationTests;

public abstract class EcommerceWebApplicationFactory(IDatabaseConfiguration databaseConfiguration)
    : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration(cfg =>
            cfg.AddInMemoryCollection(databaseConfiguration.GetConfigurationEntries()));
    }
}
