using Testcontainers.PostgreSql;

namespace Ecommerce.Shared.IntegrationTests.Database;

public class DatabaseContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("ecommerce")
        .WithUsername("admin")
        .WithPassword("admin")
        .Build();

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
