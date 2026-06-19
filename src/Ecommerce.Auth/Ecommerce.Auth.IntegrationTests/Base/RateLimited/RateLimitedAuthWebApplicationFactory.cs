using System.Globalization;
using Ecommerce.Auth.IntegrationTests.Base.Database;
using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Auth.IntegrationTests.Base.RateLimited;

// Boots a host with rate limiting enabled and a tiny login window so the throttling
// behaviour can be asserted; the shared functional host keeps the limiter disabled.
public sealed class RateLimitedAuthWebApplicationFactory(DatabaseContainerFixture containerFixture)
    : EcommerceWebApplicationFactory(new AuthDatabaseConfiguration(containerFixture.ConnectionString))
{
    public const int LoginPermitLimit = 2;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration(cfg =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimiting:Enabled"] = "true",
                ["RateLimiting:Login:PermitLimit"] = LoginPermitLimit.ToString(CultureInfo.InvariantCulture),
                ["RateLimiting:Login:WindowSeconds"] = "60",
            });
        });
    }
}
