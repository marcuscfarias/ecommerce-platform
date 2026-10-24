using Ecommerce.Catalog.Application.Products.GetProductAvailability;

namespace Ecommerce.Orders.Application.Carts;

internal static class CartItemStatusResolver
{
    public static CartItemStatus Resolve(ProductAvailability? product, int quantity)
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
