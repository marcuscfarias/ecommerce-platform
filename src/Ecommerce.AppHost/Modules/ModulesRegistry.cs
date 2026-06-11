using Ecommerce.Auth.Api;
using Ecommerce.Catalog.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.AppHost.Modules;

internal static class ModulesRegistry
{
    internal static void AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCatalogModule(configuration);
        services.AddAuthModule(configuration);
    }

    internal static void RegisterModules(this WebApplication app)
    {
        var applyMigrations = app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true);
        app.UseCatalogModule(applyMigrations);
        app.UseAuthModule(applyMigrations);
    }
}
