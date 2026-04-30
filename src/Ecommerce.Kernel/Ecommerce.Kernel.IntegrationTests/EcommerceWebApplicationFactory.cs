using Ecommerce.AppHost;
using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Kernel.IntegrationTests;

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
