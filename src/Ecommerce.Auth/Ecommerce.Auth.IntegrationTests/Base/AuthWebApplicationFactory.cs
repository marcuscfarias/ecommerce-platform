using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Auth.IntegrationTests.Base;

public sealed class AuthWebApplicationFactory(DatabaseContainerFixture containerFixture)
    : EcommerceWebApplicationFactory(new AuthDatabaseConfiguration(containerFixture.ConnectionString));
