using Ecommerce.Shared.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Shared.Infrastructure.Persistence;

public static class SharedPersistenceModule
{
    private const string ConnectionStringName = "EcommerceDb";

    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("Pagination");

        if (!section.Exists() || section.Get<PaginationSettings>() is not { PageSize: > 0 })
            throw new InvalidOperationException(
                "Pagination settings are missing or invalid. Ensure 'Pagination:PageSize' is configured with a value greater than 0.");

        services.Configure<PaginationSettings>(section);
        return services;
    }

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
