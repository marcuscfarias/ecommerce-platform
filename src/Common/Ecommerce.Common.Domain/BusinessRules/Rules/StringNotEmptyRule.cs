namespace Ecommerce.Common.Domain.BusinessRules.Rules;

public sealed class StringNotEmptyRule(string? value, string fieldName) : IRule
{
    public bool IsMet() => !string.IsNullOrWhiteSpace(value);
    public Exception CreateException() => new ArgumentNullException(fieldName, "cannot be empty.");
}