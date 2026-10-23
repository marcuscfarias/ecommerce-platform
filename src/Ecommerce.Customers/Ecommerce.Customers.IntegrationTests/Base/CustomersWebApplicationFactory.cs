using Ecommerce.Customers.IntegrationTests.Base.Database;
using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Customers.IntegrationTests.Base;

public sealed class CustomersWebApplicationFactory(DatabaseContainerFixture containerFixture)
    : EcommerceWebApplicationFactory(new CustomersDatabaseConfiguration(containerFixture.ConnectionString));
