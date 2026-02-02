namespace Ecommerce.Common.Domain.BusinessRules.Rules;

public class StringNotEmptyRule(string? value, string fieldName) : IBusinessRule
{
    public bool IsMet() => !string.IsNullOrWhiteSpace(value);
    public string Error => $"{fieldName} cannot be empty.";
}
