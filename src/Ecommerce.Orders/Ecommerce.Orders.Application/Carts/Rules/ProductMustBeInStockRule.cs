using Ecommerce.Kernel.Domain.BusinessRules;

namespace Ecommerce.Orders.Application.Carts.Rules;

internal sealed class ProductMustBeInStockRule(int stockQuantity) : IBusinessRule
{
    public bool IsMet() => stockQuantity > 0;

    public string ErrorMessage => "Product is out of stock.";
}
