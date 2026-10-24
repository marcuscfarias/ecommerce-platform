using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;
using Ecommerce.Orders.IntegrationTests.Base.Database;

namespace Ecommerce.Orders.IntegrationTests.Base;

public sealed class OrdersWebApplicationFactory(DatabaseContainerFixture containerFixture)
    : EcommerceWebApplicationFactory(new OrdersDatabaseConfiguration(containerFixture.ConnectionString));
