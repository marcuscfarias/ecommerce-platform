using Ecommerce.Kernel.Infrastructure.Persistence;
using Ecommerce.Orders.Application;
using Ecommerce.Orders.Application.Carts;
using Ecommerce.Orders.Domain.Repositories;
using Ecommerce.Orders.Infrastructure.Mediation;
using Ecommerce.Orders.Infrastructure.Persistence;
using Ecommerce.Orders.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Orders.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<OrdersDbContext>(OrdersDbContext.Schema);

        services.AddMediationModule();

        services.AddScoped<IOrderModule, OrderModule>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<CurrentCart>();
        services.AddScoped<CatalogProducts>();

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app, bool applyMigrations = false)
    {
        if (!applyMigrations)
            return app;

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        context.Database.Migrate();
        return app;
    }
}
