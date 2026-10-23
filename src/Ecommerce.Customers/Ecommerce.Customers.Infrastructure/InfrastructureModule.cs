using Ecommerce.Customers.Application;
using Ecommerce.Customers.Application.Profiles;
using Ecommerce.Customers.Domain.Repositories;
using Ecommerce.Customers.Infrastructure.Mediation;
using Ecommerce.Customers.Infrastructure.Persistence;
using Ecommerce.Customers.Infrastructure.Persistence.Repositories;
using Ecommerce.Kernel.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Customers.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<CustomersDbContext>(CustomersDbContext.Schema);

        services.AddMediationModule();

        services.AddScoped<ICustomerModule, CustomerModule>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<CurrentCustomerProfile>();

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app, bool applyMigrations = false)
    {
        if (!applyMigrations)
            return app;

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
        context.Database.Migrate();
        return app;
    }
}
