using Testcontainers.Azurite;

namespace Ecommerce.Catalog.IntegrationTests.Base.Storage;

public sealed class BlobStorageContainerFixture : IAsyncLifetime
{
    private readonly AzuriteContainer _container =
        new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:latest").Build();

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
