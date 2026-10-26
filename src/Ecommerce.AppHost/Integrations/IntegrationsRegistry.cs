using Ecommerce.AppHost.Integrations.Orders;
using Ecommerce.Orders.Domain.Catalog;

namespace Ecommerce.AppHost.Integrations;

internal static class IntegrationsRegistry
{
    // Cross-module ports are declared by the consuming module and implemented here, the only
    // project allowed to know both sides. Grouped by consumer under Integrations/<Consumer>/.
    internal static IServiceCollection AddModuleIntegrations(this IServiceCollection services)
    {
        services.AddScoped<ICatalogProducts, CatalogProductsAdapter>();
        return services;
    }
}
