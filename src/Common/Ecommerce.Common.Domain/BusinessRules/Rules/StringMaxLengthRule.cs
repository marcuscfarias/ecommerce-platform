namespace Ecommerce.Common.Domain.BusinessRules.Rules;

public sealed class StringMaxLengthRule(string? value, int maxLength, string fieldName) : IRule
{
    public bool IsMet() => value is null || value.Length <= maxLength;
    public Exception CreateException() => new ArgumentOutOfRangeException(fieldName, $"must be at most {maxLength} characters.");
}