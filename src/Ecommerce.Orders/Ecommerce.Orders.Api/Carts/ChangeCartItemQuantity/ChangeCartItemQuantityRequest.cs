using Ecommerce.Orders.Application.Carts.ChangeCartItemQuantity;

namespace Ecommerce.Orders.Api.Carts.ChangeCartItemQuantity;

public sealed record ChangeCartItemQuantityRequest(int Quantity)
{
    internal ChangeCartItemQuantityCommand ToCommand(int productId) => new(productId, Quantity);
}
