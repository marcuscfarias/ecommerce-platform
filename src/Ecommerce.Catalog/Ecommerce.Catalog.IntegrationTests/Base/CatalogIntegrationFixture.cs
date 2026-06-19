using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Catalog.IntegrationTests.Base.Storage;
using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Catalog.IntegrationTests.Base;

public sealed class CatalogIntegrationFixture : BaseIntegrationFixture<CatalogWebApplicationFactory>
{
    private readonly BlobStorageContainerFixture _blobStorage = new();

    protected override string[] Schemas => ["catalog"];

    public async Task<string> UploadImageAsync(byte[] content, string contentType)
    {
        using var scope = Factory.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IProductImageStorage>();
        using var stream = new MemoryStream(content);
        return await storage.UploadAsync(stream, contentType);
    }

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
