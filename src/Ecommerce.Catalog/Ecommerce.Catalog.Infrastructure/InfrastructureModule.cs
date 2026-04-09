using Ecommerce.Catalog.Application;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Infrastructure.Mediation;
using Ecommerce.Catalog.Infrastructure.Persistence;
using Ecommerce.Catalog.Infrastructure.Persistence.Repositories;
using Ecommerce.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Catalog.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<CatalogDbContext>(CatalogDbContext.Schema);

        services.AddMediationModule();

        services.AddScoped<ICatalogModule, CatalogModule>();
        services.AddScoped<ICatalogRepository, CategoryRepository>();

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app, bool applyMigrations = false)
    {
        if (!applyMigrations)
            return app;

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        context.Database.Migrate();
        return app;
    }
}