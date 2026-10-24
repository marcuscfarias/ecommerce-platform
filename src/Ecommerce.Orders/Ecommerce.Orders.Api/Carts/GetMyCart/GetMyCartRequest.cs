using Ecommerce.Orders.Application.Carts.GetMyCart;

namespace Ecommerce.Orders.Api.Carts.GetMyCart;

public sealed record GetMyCartRequest
{
    internal static GetMyCartQuery ToQuery() => new();
}
