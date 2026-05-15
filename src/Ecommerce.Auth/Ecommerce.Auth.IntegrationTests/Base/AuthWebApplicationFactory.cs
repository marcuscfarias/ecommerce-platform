using System.Globalization;
using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Auth.IntegrationTests.Base;

public sealed class AuthWebApplicationFactory(DatabaseContainerFixture containerFixture)
    : EcommerceWebApplicationFactory(new AuthDatabaseConfiguration(containerFixture.ConnectionString))
{
    // Deterministic JWT config for tests. SecretKey must be >= 32 bytes (HS256 minimum).
    internal const string TestSecretKey = "integration-test-secret-key-with-at-least-32-bytes-for-hs256!!";
    internal const string TestIssuer = "ecommerce-platform";
    internal const string TestAudience = "ecommerce-platform";
    internal const int TestAccessTokenMinutes = 15;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration(cfg =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Jwt:SecretKey"] = TestSecretKey,
                ["Auth:Jwt:Issuer"] = TestIssuer,
                ["Auth:Jwt:Audience"] = TestAudience,
                ["Auth:Jwt:AccessTokenMinutes"] = TestAccessTokenMinutes.ToString(CultureInfo.InvariantCulture),
                // Lower bcrypt cost so seeded hashes verify quickly in tests.
                ["Auth:Password:BcryptWorkFactor"] = "4",
            }));
    }
}
