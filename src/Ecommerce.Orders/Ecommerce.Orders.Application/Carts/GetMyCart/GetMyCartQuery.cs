using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Orders.Application.Carts.GetMyCart;

public sealed record GetMyCartQuery : IQuery<GetMyCartResult>;
