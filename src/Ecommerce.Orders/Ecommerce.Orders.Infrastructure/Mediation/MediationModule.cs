using Ecommerce.Kernel.Infrastructure.Mediator;
using Ecommerce.Orders.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Orders.Infrastructure.Mediation;

internal static class MediationModule
{
    internal static IServiceCollection AddMediationModule(this IServiceCollection services)
    {
        services.AddMediator(typeof(IOrderModule).Assembly);
        return services;
    }
}
