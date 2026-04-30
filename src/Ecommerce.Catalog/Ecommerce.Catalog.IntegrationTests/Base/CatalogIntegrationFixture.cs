using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Catalog.IntegrationTests.Base;

public sealed class CatalogIntegrationFixture : BaseIntegrationFixture<CatalogWebApplicationFactory>
{
    protected override string[] Schemas => ["catalog"];

    protected override CatalogWebApplicationFactory CreateFactory(DatabaseContainerFixture container)
        => new(container);
}
