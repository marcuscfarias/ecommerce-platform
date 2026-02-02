namespace Ecommerce.Common.Domain.BusinessRules;

public class BusinessRuleValidationException(string message) : InvalidOperationException(message)
{
}