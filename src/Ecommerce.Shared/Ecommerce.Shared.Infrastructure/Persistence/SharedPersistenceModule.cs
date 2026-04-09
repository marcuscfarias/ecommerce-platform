using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Shared.Infrastructure.Persistence;

public static class SharedPersistenceModule
{
    private const string ConnectionStringName = "EcommerceDb";

    public static IServiceCollection AddModuleDbContext<TContext>(
        this IServiceCollection services,
        string schema)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>((sp, options) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>()
                .GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{ConnectionStringName}' is not configured.");

            options.UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema));
        });

        return services;
    }
}
