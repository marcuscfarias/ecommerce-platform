namespace Ecommerce.Common.Domain.BusinessRules.Rules;

public class BusinessRulesValidationException(string message) : InvalidOperationException(message)
{
}