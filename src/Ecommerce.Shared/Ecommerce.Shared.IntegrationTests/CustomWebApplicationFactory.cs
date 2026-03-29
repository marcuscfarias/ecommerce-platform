using Ecommerce.AppHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Ecommerce.Shared.IntegrationTests;

public abstract class CustomWebApplicationFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("test")
        .WithUsername("admin")
        .WithPassword("admin")
        .Build();

    public HttpClient HttpClient { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings:Postgres", _dbContainer.GetConnectionString());
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        HttpClient = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}