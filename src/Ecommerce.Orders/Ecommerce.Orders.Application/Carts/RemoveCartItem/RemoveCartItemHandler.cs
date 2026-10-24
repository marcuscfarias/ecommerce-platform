using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Orders.Application.Carts.RemoveCartItem;

internal sealed class RemoveCartItemHandler(CurrentCart currentCart) : IRequestHandler<RemoveCartItemCommand>
{
    public async Task Handle(RemoveCartItemCommand command, CancellationToken cancellationToken)
    {
        var cart = await currentCart.FindAsync(cancellationToken);
        if (cart?.FindItem(command.ProductId) is null)
            throw new ResourceNotFoundException("Cart item", command.ProductId);

        cart.RemoveItem(command.ProductId);
        await currentCart.SaveChangesAsync(cancellationToken);
    }
}
