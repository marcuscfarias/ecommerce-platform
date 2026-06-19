using Ecommerce.Catalog.IntegrationTests.Base.Storage;
using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Catalog.IntegrationTests.Base;

public sealed class CatalogIntegrationFixture : BaseIntegrationFixture<CatalogWebApplicationFactory>
{
    private readonly BlobStorageContainerFixture _blobStorage = new();

    protected override string[] Schemas => ["catalog"];

    public override async Task InitializeAsync()
    {
        await _blobStorage.InitializeAsync();
        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _blobStorage.DisposeAsync();
    }

    protected override CatalogWebApplicationFactory CreateFactory(DatabaseContainerFixture container)
        => new(container, _blobStorage.ConnectionString);
}
