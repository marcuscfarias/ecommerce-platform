using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Orders.Application.Carts.ChangeCartItemQuantity;

public sealed record ChangeCartItemQuantityCommand(int ProductId, int Quantity) : ICommand;
