using Ecommerce.AppHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Shared.IntegrationTests;

public abstract class CustomWebApplicationFactory(string connectionString)
    : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration(cfg =>
            cfg.AddInMemoryCollection([
                new("ConnectionStrings:Postgres", connectionString)
            ]));
    }
}
