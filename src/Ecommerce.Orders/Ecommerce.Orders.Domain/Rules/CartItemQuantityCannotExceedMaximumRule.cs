using Ecommerce.Kernel.Domain.BusinessRules;
using Ecommerce.Orders.Domain.Entities;

namespace Ecommerce.Orders.Domain.Rules;

internal sealed class CartItemQuantityCannotExceedMaximumRule(int quantity) : IBusinessRule
{
    public bool IsMet() => quantity <= CartConsts.MaxItemQuantity;

    public string ErrorMessage => $"Maximum of {CartConsts.MaxItemQuantity} units per product.";
}
