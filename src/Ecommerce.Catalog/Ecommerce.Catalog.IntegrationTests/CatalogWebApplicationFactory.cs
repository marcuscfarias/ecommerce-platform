using Ecommerce.Shared.IntegrationTests;
using Ecommerce.Shared.IntegrationTests.Database;

namespace Ecommerce.Catalog.IntegrationTests;

public sealed class CatalogWebApplicationFactory(DatabaseContainerFixture containerFixture)
    : EcommerceWebApplicationFactory(new CatalogDatabaseConfiguration(containerFixture.ConnectionString));
