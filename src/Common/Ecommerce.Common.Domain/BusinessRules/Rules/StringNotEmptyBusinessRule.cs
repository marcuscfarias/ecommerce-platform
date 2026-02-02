namespace Ecommerce.Common.Domain.BusinessRules.Rules;

public class StringNotEmptyBusinessRule(string? value, string fieldName) : IBusinessRule
{
    public bool IsMet() => !string.IsNullOrWhiteSpace(value);
    public string Error => $"{fieldName} cannot be empty.";
    public Exception CreateException(string errorMessage) => new BusinessRulesValidationException(errorMessage);
}