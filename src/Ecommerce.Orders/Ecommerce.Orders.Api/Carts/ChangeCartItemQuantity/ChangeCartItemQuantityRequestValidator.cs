using Ecommerce.Orders.Domain.Entities;
using FluentValidation;

namespace Ecommerce.Orders.Api.Carts.ChangeCartItemQuantity;

internal sealed class ChangeCartItemQuantityRequestValidator : AbstractValidator<ChangeCartItemQuantityRequest>
{
    public ChangeCartItemQuantityRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .InclusiveBetween(CartConsts.MinItemQuantity, CartConsts.MaxItemQuantity);
    }
}
