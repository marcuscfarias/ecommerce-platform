using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Orders.Application.Carts.AddCartItem;

public sealed record AddCartItemCommand(int ProductId, int Quantity) : ICommand;
