using Ecommerce.Auth.Application;
using Ecommerce.Auth.Application.Users.Security;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Auth.Infrastructure.Mediation;
using Ecommerce.Auth.Infrastructure.Persistence;
using Ecommerce.Auth.Infrastructure.Persistence.Repositories;
using Ecommerce.Auth.Infrastructure.Security;
using Ecommerce.Kernel.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Auth.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<AuthDbContext>(AuthDbContext.Schema);

        services.AddMediationModule();

        services.AddScoped<IAuthModule, AuthModule>();
        services.AddScoped<IAuthRepository, UserRepository>();

        services.Configure<AuthPasswordSettings>(configuration.GetSection("Auth:Password"));
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app, bool applyMigrations = false)
    {
        if (!applyMigrations)
            return app;

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        context.Database.Migrate();
        return app;
    }
}
