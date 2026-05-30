using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.Infrastructure;
using Ecommerce.Kernel.Application.Security;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Auth.Api;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddApplicationPart(typeof(AuthModule).Assembly);
        services.AddValidatorsFromAssembly(typeof(AuthModule).Assembly, includeInternalTypes: true);
        services.AddInfrastructure(configuration);

        // The Auth module owns the policies guarding its own endpoints; they read the
        // permission claim, never roles, so the module stays unaware of role semantics.
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.CanManageUsers,
                policy => policy.RequireClaim(AppClaimTypes.Permission, AuthPermissions.ManageUsers))
            .AddPolicy(AuthPolicies.CanViewUsers,
                policy => policy.RequireClaim(AppClaimTypes.Permission, AuthPermissions.ViewUsers));

        return services;
    }

    public static IApplicationBuilder UseAuthModule(this IApplicationBuilder app, bool applyMigrations = false)
    {
        app.UseInfrastructure(applyMigrations);
        return app;
    }
}
