using Ecommerce.Orders.Application.Carts.RemoveCartItem;

namespace Ecommerce.Orders.Api.Carts.RemoveCartItem;

public sealed record RemoveCartItemRequest
{
    internal static RemoveCartItemCommand ToCommand(int productId) => new(productId);
}
