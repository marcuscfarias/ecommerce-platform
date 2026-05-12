using Ecommerce.Auth.Infrastructure;
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
        return services;
    }

    public static IApplicationBuilder UseAuthModule(this IApplicationBuilder app, bool applyMigrations = false)
    {
        app.UseInfrastructure(applyMigrations);
        return app;
    }
}
