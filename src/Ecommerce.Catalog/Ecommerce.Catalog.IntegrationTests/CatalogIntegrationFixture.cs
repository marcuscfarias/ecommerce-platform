using Npgsql;
using Respawn;
using Ecommerce.Shared.IntegrationTests.Database;

namespace Ecommerce.Catalog.IntegrationTests;

public sealed class CatalogIntegrationFixture : IAsyncLifetime
{
    private readonly DatabaseContainerFixture _containerFixture;
    private Respawner _respawner = null!;

    public CatalogWebApplicationFactory Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public CatalogIntegrationFixture(DatabaseContainerFixture containerFixture)
    {
        _containerFixture = containerFixture;
    }

    public async Task InitializeAsync()
    {
        Factory = new CatalogWebApplicationFactory(_containerFixture);

        // CreateClient triggers WebApplicationFactory startup, which applies EF migrations.
        // Respawner must be initialized after the schema exists.
        Client = Factory.CreateClient();

        await using var connection = new NpgsqlConnection(_containerFixture.ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["catalog"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_containerFixture.ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }
}
