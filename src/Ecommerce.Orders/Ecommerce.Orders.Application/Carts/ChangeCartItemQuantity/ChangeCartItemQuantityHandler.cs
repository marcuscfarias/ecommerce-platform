using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.BusinessRules;
using Ecommerce.Orders.Application.Carts.Rules;
using MediatR;

namespace Ecommerce.Orders.Application.Carts.ChangeCartItemQuantity;

internal sealed class ChangeCartItemQuantityHandler(CurrentCart currentCart, CatalogProducts catalogProducts)
    : IRequestHandler<ChangeCartItemQuantityCommand>
{
    public async Task Handle(ChangeCartItemQuantityCommand command, CancellationToken cancellationToken)
    {
        var cart = await currentCart.FindAsync(cancellationToken);
        if (cart?.FindItem(command.ProductId) is null)
            throw new ResourceNotFoundException("Cart item", command.ProductId);

        var products = await catalogProducts.GetAsync([command.ProductId], cancellationToken);
        var product = products.GetValueOrDefault(command.ProductId);

        BusinessRule.Validate(new ProductMustBeAvailableRule(product is { IsActive: true }));
        BusinessRule.Validate(new ProductMustBeInStockRule(product!.StockQuantity));
        BusinessRule.Validate(new StockMustCoverQuantityRule(product.StockQuantity, command.Quantity));

        cart.ChangeItemQuantity(command.ProductId, command.Quantity);
        await currentCart.SaveChangesAsync(cancellationToken);
    }
}
