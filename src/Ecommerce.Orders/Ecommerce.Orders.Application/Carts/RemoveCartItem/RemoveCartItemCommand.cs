using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Orders.Application.Carts.RemoveCartItem;

public sealed record RemoveCartItemCommand(int ProductId) : ICommand;
