using Ecommerce.Kernel.Domain.BusinessRules;

namespace Ecommerce.Orders.Application.Carts.Rules;

internal sealed class StockMustCoverQuantityRule(int stockQuantity, int quantity) : IBusinessRule
{
    public bool IsMet() => quantity <= stockQuantity;

    public string ErrorMessage => $"Only {stockQuantity} available.";
}
