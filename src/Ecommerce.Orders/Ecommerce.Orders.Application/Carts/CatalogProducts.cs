using Ecommerce.Catalog.Application;
using Ecommerce.Catalog.Application.Products.GetProductAvailability;

namespace Ecommerce.Orders.Application.Carts;

internal sealed class CatalogProducts(ICatalogModule catalogModule)
{
    public async Task<IReadOnlyDictionary<int, ProductAvailability>> GetAsync(
        IReadOnlyCollection<int> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return new Dictionary<int, ProductAvailability>();

        var result = await catalogModule.ExecuteQueryAsync(
            new GetProductAvailabilityQuery(productIds), cancellationToken);

        return result.Items.ToDictionary(p => p.Id);
    }
}
