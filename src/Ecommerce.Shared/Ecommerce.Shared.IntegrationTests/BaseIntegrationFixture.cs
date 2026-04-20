using Ecommerce.Shared.IntegrationTests.Database;

namespace Ecommerce.Shared.IntegrationTests;

public abstract class BaseIntegrationFixture<TFactory> : IAsyncLifetime
    where TFactory : EcommerceWebApplicationFactory
{
    private readonly DatabaseContainerFixture _container = new();
    private DatabaseResetter _resetter = null!;

    protected TFactory Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    protected abstract TFactory CreateFactory(DatabaseContainerFixture container);
    protected abstract string[] Schemas { get; }

    public async Task InitializeAsync()
    {
        await _container.InitializeAsync();
        Factory = CreateFactory(_container);
        // CreateClient triggers WebApplicationFactory startup, which applies EF migrations.
        // The resetter must be initialized after the schema exists.
        Client = Factory.CreateClient();

        _resetter = new DatabaseResetter(_container.ConnectionString, Schemas);
        await _resetter.InitializeAsync();
    }

    public Task ResetDatabaseAsync() => _resetter.ResetAsync();

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await _resetter.DisposeAsync();
        await _container.DisposeAsync();
    }
}
