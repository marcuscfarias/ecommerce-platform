using Npgsql;
using Respawn;

namespace Ecommerce.Shared.IntegrationTests.Database;

public sealed class DatabaseResetter(string connectionString, string[] schemas) : IAsyncDisposable
{
    private NpgsqlConnection _connection = null!;
    private Respawner _respawner = null!;

    public async Task InitializeAsync()
    {
        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = schemas
        });
    }

    public Task ResetAsync() => _respawner.ResetAsync(_connection);

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}
