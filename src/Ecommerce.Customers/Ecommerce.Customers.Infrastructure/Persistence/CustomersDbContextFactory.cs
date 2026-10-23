using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Customers.Infrastructure.Persistence;

internal sealed class CustomersDbContextFactory : IDesignTimeDbContextFactory<CustomersDbContext>
{
    public CustomersDbContext CreateDbContext(string[] args)
    {
        var appHostPath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Ecommerce.AppHost"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(appHostPath)
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var connectionString = configuration.GetConnectionString("EcommerceDb")
            ?? throw new InvalidOperationException("Connection string 'EcommerceDb' is not configured.");

        var options = new DbContextOptionsBuilder<CustomersDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CustomersDbContext(options);
    }
}
