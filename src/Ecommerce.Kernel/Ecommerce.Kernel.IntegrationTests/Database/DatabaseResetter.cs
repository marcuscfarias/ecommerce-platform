using Microsoft.Data.SqlClient;
using Respawn;

namespace Ecommerce.Kernel.IntegrationTests.Database;

public sealed class DatabaseResetter(string connectionString, string[] schemas) : IAsyncDisposable
{
    private SqlConnection _connection = null!;
    private Respawner _respawner = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqlConnection(connectionString);
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = schemas
        });
    }

    public Task ResetAsync() => _respawner.ResetAsync(_connection);

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}
