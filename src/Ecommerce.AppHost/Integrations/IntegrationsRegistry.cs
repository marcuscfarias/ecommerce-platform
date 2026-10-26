using Ecommerce.AppHost.Integrations.Orders;
using Ecommerce.Orders.Domain.Catalog;

namespace Ecommerce.AppHost.Integrations;

internal static class IntegrationsRegistry
{
    internal static IServiceCollection AddModuleIntegrations(this IServiceCollection services)
    {
        services.AddScoped<ICatalogProducts, CatalogProductsAdapter>();
        return services;
    }
}
