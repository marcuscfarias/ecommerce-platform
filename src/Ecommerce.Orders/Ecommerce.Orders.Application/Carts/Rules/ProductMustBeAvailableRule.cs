using Ecommerce.Kernel.Domain.BusinessRules;

namespace Ecommerce.Orders.Application.Carts.Rules;

internal sealed class ProductMustBeAvailableRule(bool isActive) : IBusinessRule
{
    public bool IsMet() => isActive;

    public string ErrorMessage => "Product is no longer available.";
}
