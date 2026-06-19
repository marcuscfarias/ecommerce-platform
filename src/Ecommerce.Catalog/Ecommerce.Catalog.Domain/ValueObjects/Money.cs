using Ecommerce.Catalog.Domain.Rules;
using Ecommerce.Kernel.Domain.BusinessRules;
using Ecommerce.Kernel.Domain.ValueObjects;

namespace Ecommerce.Catalog.Domain.ValueObjects;

public sealed record Money : ValueObject
{
    private const string DefaultCurrency = "USD";

    public decimal Amount { get; }
    public string Currency { get; } = DefaultCurrency;

    public Money(decimal amount)
    {
        BusinessRule.Validate(new AmountCannotBeNegativeRule(amount));

        Amount = amount;
    }
}
