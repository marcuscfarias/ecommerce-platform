using Ecommerce.AppHost;
using Ecommerce.Kernel.IntegrationTests.Database;
using Ecommerce.Kernel.IntegrationTests.JwtToken;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Kernel.IntegrationTests;

public abstract class EcommerceWebApplicationFactory(IDatabaseConfiguration databaseConfiguration)
    : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Container connection string, visible to Program.Main where the host reads it
        foreach (var (key, value) in databaseConfiguration.GetConfigurationEntries())
        {
            builder.UseSetting(key, value);
        }

        ConfigureMessaging(builder);

        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration(cfg =>
        {
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
                ["Auth:EmailConfirmation:ApiBaseUrl"] = "https://api.test.local",
                ["Auth:EmailConfirmation:ShopBaseUrl"] = "https://shop.test.local",

                // The composed host validates Email on startup and the committed appsettings.json
                // selects Resend with an empty key, so the test host has to pick the console
                // adapter explicitly or every suite dies before the first request.
                ["Email:Provider"] = "Console",
                ["Email:FromAddress"] = "noreply@test.local",

                // The composed host now wires the global rate limiter (default on). Disable it
                // for every functional suite so legitimate test traffic is never throttled.
                ["RateLimiting:Enabled"] = "false",
            });
        });
    }

    protected virtual void ConfigureMessaging(IWebHostBuilder builder)
    {
        // Placeholder broker address that only satisfies the boot validation
        builder.UseSetting("ConnectionStrings:RabbitMq", "amqp://localhost");
        // No RabbitMQ in module suites: the bus runs on the in-memory transport
        builder.ConfigureServices(services => services.AddMassTransitTestHarness());
    }
}
