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
        {
            cfg.AddInMemoryCollection(databaseConfiguration.GetConfigurationEntries());
            cfg.AddInMemoryCollection(TestJwtDefaults.AsConfiguration());

            // Every test host boots the whole composed AppHost, which includes the Auth
            // module (validates Auth:AdminSeed on startup and seeds an admin). CI has no
            // appsettings.Development.json, so the shared harness supplies the boot config
            // here — once — for every bounded-context's test host.
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Password:BcryptWorkFactor"] = "4",
                ["Auth:AdminSeed:Email"] = "admin@test.local",
                ["Auth:AdminSeed:Password"] = "TestAdminP@ss1",
                ["Auth:AdminSeed:Name"] = "Test Admin",

                // The composed host now wires the global rate limiter (default on). Disable it
                // for every functional suite so legitimate test traffic is never throttled.
                ["RateLimiting:Enabled"] = "false",
            });
        });
    }
}
