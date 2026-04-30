using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Catalog.IntegrationTests.Base;

public sealed class CatalogWebApplicationFactory(DatabaseContainerFixture containerFixture)
    : EcommerceWebApplicationFactory(new CatalogDatabaseConfiguration(containerFixture.ConnectionString));
