using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Auth.IntegrationTests.Base;

public sealed class AuthWebApplicationFactory(DatabaseContainerFixture containerFixture)
    : EcommerceWebApplicationFactory(new AuthDatabaseConfiguration(containerFixture.ConnectionString))
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration(cfg =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Lower bcrypt cost so seeded hashes verify quickly in tests.
                ["Auth:Password:BcryptWorkFactor"] = "4",
            }));
    }
}
