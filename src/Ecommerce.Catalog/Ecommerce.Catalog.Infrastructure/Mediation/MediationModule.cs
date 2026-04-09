using Ecommerce.Catalog.Application;
using Ecommerce.Shared.Infrastructure.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Catalog.Infrastructure.Mediation;

internal static class MediationModule
{
    internal static IServiceCollection AddMediationModule(this IServiceCollection services)
    {
        services.AddMediator(typeof(ICatalogModule).Assembly);
        return services;
    }
}
