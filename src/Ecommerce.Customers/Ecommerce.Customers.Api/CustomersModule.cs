using Ecommerce.Customers.Api.Authorization;
using Ecommerce.Customers.Infrastructure;
using Ecommerce.Kernel.Application.Security;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Customers.Api;

public static class CustomersModule
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddApplicationPart(typeof(CustomersModule).Assembly);
        services.AddValidatorsFromAssembly(typeof(CustomersModule).Assembly, includeInternalTypes: true);
        services.AddInfrastructure(configuration);

        services.AddAuthorizationBuilder()
            .AddPolicy(CustomersPolicies.CanManageProfile,
                policy => policy.RequireClaim(AppClaimTypes.Permission, CustomersPermissions.ManageProfile));

        return services;
    }

    public static IApplicationBuilder UseCustomersModule(this IApplicationBuilder app, bool applyMigrations = false)
    {
        app.UseInfrastructure(applyMigrations);
        return app;
    }
}
