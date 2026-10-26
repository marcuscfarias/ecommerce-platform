using Ecommerce.Orders.Domain.Catalog;
using Ecommerce.Orders.Domain.Entities;
using MediatR;

namespace Ecommerce.Orders.Application.Carts.GetMyCart;

internal sealed class GetMyCartHandler(CurrentCart currentCart, ICatalogProducts catalogProducts)
    : IRequestHandler<GetMyCartQuery, GetMyCartResult>
{
    public async Task<GetMyCartResult> Handle(GetMyCartQuery query, CancellationToken cancellationToken)
    {
        var cart = await currentCart.FindAsync(cancellationToken);
        if (cart is null)
            return new GetMyCartResult([], 0, 0m);

        var lines = cart.Items.OrderBy(i => i.Id).ToList();
        var products = await catalogProducts.GetAsync(lines.ConvertAll(i => i.ProductId), cancellationToken);

        var items = lines.ConvertAll(line => ToItemResult(line, products.GetValueOrDefault(line.ProductId)));

        return new GetMyCartResult(
            items,
            items.Sum(i => i.Quantity),
            items.Where(i => i.Status == CartItemStatus.Available).Sum(i => i.Subtotal));
    }

    private static GetMyCartItemResult ToItemResult(CartItem line, CatalogProduct? product)
    {
        var status = CartItemStatusResolver.Resolve(product, line.Quantity);
        var unitPrice = product?.Price ?? 0m;

        return new GetMyCartItemResult(
            line.ProductId,
            product?.Name ?? string.Empty,
            unitPrice,
            line.Quantity,
            unitPrice * line.Quantity,
            product?.HasImage ?? false,
            status,
            status == CartItemStatus.InsufficientStock ? product?.StockQuantity : null);
    }
}
