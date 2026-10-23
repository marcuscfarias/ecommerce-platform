using Ecommerce.Customers.Application;
using Ecommerce.Kernel.Infrastructure.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Customers.Infrastructure.Mediation;

internal static class MediationModule
{
    internal static IServiceCollection AddMediationModule(this IServiceCollection services)
    {
        services.AddMediator(typeof(ICustomerModule).Assembly);
        return services;
    }
}
