using Ecommerce.Shared.IntegrationTests;
using Ecommerce.Shared.IntegrationTests.Database;

namespace Ecommerce.Catalog.IntegrationTests.Base;

public sealed class CatalogIntegrationFixture : BaseIntegrationFixture<CatalogWebApplicationFactory>
{
    protected override string[] Schemas => ["catalog"];

    protected override CatalogWebApplicationFactory CreateFactory(DatabaseContainerFixture container)
        => new(container);
}
