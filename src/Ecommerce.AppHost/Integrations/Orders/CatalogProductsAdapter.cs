using Ecommerce.Catalog.Application;
using Ecommerce.Catalog.Application.Products.GetProductAvailability;
using Ecommerce.Orders.Domain.Catalog;

namespace Ecommerce.AppHost.Integrations.Orders;

internal sealed class CatalogProductsAdapter(ICatalogModule catalogModule) : ICatalogProducts
{
    public async Task<IReadOnlyDictionary<int, CatalogProduct>> GetAsync(
        IReadOnlyCollection<int> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return new Dictionary<int, CatalogProduct>();

        var result = await catalogModule.ExecuteQueryAsync(
            new GetProductAvailabilityQuery(productIds), cancellationToken);

        return result.Items.ToDictionary(p => p.Id, ToCatalogProduct);
    }

    private static CatalogProduct ToCatalogProduct(ProductAvailability product) =>
        new(product.Id, product.Name, product.Price, product.StockQuantity, product.IsActive, product.HasImage);
}
