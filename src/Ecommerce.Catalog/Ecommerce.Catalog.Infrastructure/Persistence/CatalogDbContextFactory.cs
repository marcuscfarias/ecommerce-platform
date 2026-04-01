using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Catalog.Infrastructure.Persistence;

internal sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var appHostDir = FindAppHostDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(appHostDir)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("CatalogDb")
            ?? throw new InvalidOperationException(
                "Connection string 'CatalogDb' not found in appsettings.Development.json or environment variables.");

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", CatalogDbContext.Schema))
            .Options;

        return new CatalogDbContext(options);
    }

    private static string FindAppHostDirectory()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            if (dir.GetFiles("*.sln").Length > 0)
                return Path.Combine(dir.FullName, "src", "Ecommerce.AppHost");
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Could not locate solution root to find appsettings.");
    }
}
