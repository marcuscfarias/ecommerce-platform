using Ecommerce.Orders.Application.Carts.AddCartItem;

namespace Ecommerce.Orders.Api.Carts.AddCartItem;

public sealed record AddCartItemRequest(int ProductId, int Quantity)
{
    internal AddCartItemCommand ToCommand() => new(ProductId, Quantity);
}
