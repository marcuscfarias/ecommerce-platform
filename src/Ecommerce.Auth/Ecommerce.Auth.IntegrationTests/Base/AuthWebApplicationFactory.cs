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
                ["Auth:Password:BcryptWorkFactor"] = "4",
                ["Auth:AdminSeed:Email"] = TestAdminDefaults.Email,
                ["Auth:AdminSeed:Password"] = TestAdminDefaults.Password,
                ["Auth:AdminSeed:Name"] = TestAdminDefaults.Name,
            }));
    }
}
