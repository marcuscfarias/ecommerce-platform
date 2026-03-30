using Ecommerce.Shared.IntegrationTests.Database;
using Npgsql;
using Respawn;

namespace Ecommerce.Catalog.IntegrationTests.Base;

public sealed class CatalogIntegrationFixture : IAsyncLifetime
{
    private readonly DatabaseContainerFixture _containerFixture = new();
    private Respawner _respawner = null!;

    private CatalogWebApplicationFactory Factory { get; set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _containerFixture.InitializeAsync();

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
        await _containerFixture.DisposeAsync();
    }
}
