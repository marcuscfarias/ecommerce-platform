namespace Ecommerce.Common.Domain.BusinessRules.Rules;

public sealed class StringMaxLengthBusinessRule(string? value, int maxLength, string fieldName) : IBusinessRule
{
    //if there's a value, check its length.
    public bool IsMet() => value is null || value.Length <= maxLength;
    public string Error => $"{fieldName} must be at most {maxLength} characters.";
    public Exception CreateException(string errorMessage) => new BusinessRulesValidationException(errorMessage);
}