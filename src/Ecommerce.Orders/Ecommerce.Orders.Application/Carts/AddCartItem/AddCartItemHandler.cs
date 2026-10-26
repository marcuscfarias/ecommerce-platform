using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.BusinessRules;
using Ecommerce.Orders.Domain.Catalog;
using Ecommerce.Orders.Application.Carts.Rules;
using MediatR;

namespace Ecommerce.Orders.Application.Carts.AddCartItem;

internal sealed class AddCartItemHandler(CurrentCart currentCart, ICatalogProducts catalogProducts)
    : IRequestHandler<AddCartItemCommand>
{
    public async Task Handle(AddCartItemCommand command, CancellationToken cancellationToken)
    {
        var products = await catalogProducts.GetAsync([command.ProductId], cancellationToken);
        if (!products.TryGetValue(command.ProductId, out var product) || !product.IsActive)
            throw new ResourceNotFoundException("Product", command.ProductId);

        var cart = await currentCart.GetOrCreateAsync(cancellationToken);
        cart.AddItem(command.ProductId, command.Quantity);
        var resultingQuantity = cart.FindItem(command.ProductId)!.Quantity;

        BusinessRule.Validate(new ProductMustBeInStockRule(product.StockQuantity));
        BusinessRule.Validate(new StockMustCoverQuantityRule(product.StockQuantity, resultingQuantity));

        await currentCart.SaveChangesAsync(cancellationToken);
    }
}
