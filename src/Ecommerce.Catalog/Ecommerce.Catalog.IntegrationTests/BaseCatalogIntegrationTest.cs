namespace Ecommerce.Catalog.IntegrationTests;

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
}
