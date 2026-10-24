using Ecommerce.Orders.Domain.Entities;
using FluentValidation;

namespace Ecommerce.Orders.Api.Carts.AddCartItem;

internal sealed class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.Quantity)
            .InclusiveBetween(CartConsts.MinItemQuantity, CartConsts.MaxItemQuantity);
    }
}
