using Ecommerce.Shared.IntegrationTests.Database;

namespace Ecommerce.Catalog.IntegrationTests;

[CollectionDefinition(nameof(CatalogTestCollection))]
public class CatalogTestCollection
    : ICollectionFixture<DatabaseContainerFixture>,
      ICollectionFixture<CatalogIntegrationFixture>;
