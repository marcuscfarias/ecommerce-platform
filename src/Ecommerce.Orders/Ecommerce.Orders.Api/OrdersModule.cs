using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Orders.Api;

public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddApplicationPart(typeof(OrdersModule).Assembly);
        services.AddValidatorsFromAssembly(typeof(OrdersModule).Assembly, includeInternalTypes: true);

        return services;
    }

    public static IApplicationBuilder UseOrdersModule(this IApplicationBuilder app, bool applyMigrations = false)
    {
        return app;
    }
}
