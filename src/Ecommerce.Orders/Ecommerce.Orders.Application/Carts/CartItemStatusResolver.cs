using Ecommerce.Orders.Domain.Catalog;

namespace Ecommerce.Orders.Application.Carts;

internal static class CartItemStatusResolver
{
    public static CartItemStatus Resolve(CatalogProduct? product, int quantity)
    {
        if (product is not { IsActive: true })
            return CartItemStatus.Unavailable;

        if (product.StockQuantity == 0)
            return CartItemStatus.OutOfStock;

        return quantity > product.StockQuantity
            ? CartItemStatus.InsufficientStock
            : CartItemStatus.Available;
    }
}
