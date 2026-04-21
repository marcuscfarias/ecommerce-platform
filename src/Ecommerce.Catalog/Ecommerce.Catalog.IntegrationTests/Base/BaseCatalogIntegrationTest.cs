using Ecommerce.Catalog.Infrastructure.Persistence;

namespace Ecommerce.Catalog.IntegrationTests.Base;

[Collection(nameof(CatalogTestCollection))]
public abstract class BaseCatalogIntegrationTest
{
    protected HttpClient Client { get; }
    private readonly CatalogIntegrationFixture _fixture;

    protected BaseCatalogIntegrationTest(CatalogIntegrationFixture fixture)
    {
        _fixture = fixture;
        Client = fixture.Client;
    }

    protected Task ResetDatabaseAsync() => _fixture.ResetDatabaseAsync();

    internal Task SeedAsync(Func<CatalogDbContext, Task> seed) => _fixture.SeedAsync(seed);
}
