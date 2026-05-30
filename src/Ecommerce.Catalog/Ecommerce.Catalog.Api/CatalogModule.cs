using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Kernel.Application.Security;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Catalog.Api;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddApplicationPart(typeof(CatalogModule).Assembly);
        services.AddValidatorsFromAssembly(typeof(CatalogModule).Assembly, includeInternalTypes: true);
        services.AddInfrastructure(configuration);

        services.AddAuthorizationBuilder()
            .AddPolicy(CatalogPolicies.CanManageCatalog,
                policy => policy.RequireClaim(AppClaimTypes.Permission, CatalogPermissions.Manage))
            .AddPolicy(CatalogPolicies.CanViewCatalog,
                policy => policy.RequireClaim(AppClaimTypes.Permission, CatalogPermissions.View));

        return services;
    }

    public static IApplicationBuilder UseCatalogModule(this IApplicationBuilder app, bool applyMigrations = false)
    {
        app.UseInfrastructure(applyMigrations);
        return app;
    }
}
