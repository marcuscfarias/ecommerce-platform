using Ecommerce.Kernel.Domain.BusinessRules;

namespace Ecommerce.Catalog.Domain.Rules;

internal sealed class StockQuantityCannotBeNegativeRule(int stockQuantity) : IBusinessRule
{
    public bool IsMet() => stockQuantity >= 0;

    public string ErrorMessage => "Stock quantity cannot be negative.";
}
