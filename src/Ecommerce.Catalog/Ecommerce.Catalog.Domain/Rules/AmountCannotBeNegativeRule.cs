using Ecommerce.Kernel.Domain.BusinessRules;

namespace Ecommerce.Catalog.Domain.Rules;

internal sealed class AmountCannotBeNegativeRule(decimal amount) : IBusinessRule
{
    public bool IsMet() => amount >= 0;

    public string ErrorMessage => "Amount cannot be negative.";
}
