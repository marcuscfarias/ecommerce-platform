namespace Ecommerce.Common.Domain.BusinessRules.Rules;

public sealed class BusinessRulesValidationException(string message) : InvalidOperationException(message)
{
}