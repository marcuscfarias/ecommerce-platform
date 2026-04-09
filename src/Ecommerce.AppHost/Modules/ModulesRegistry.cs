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
    }

    internal static void RegisterModules(this WebApplication app)
    {
        app.UseCatalogModule(applyMigrations: app.Environment.IsDevelopment());
    }
}
