namespace Ecommerce.Shared.Domain.BusinessRules;

public class BusinessRuleValidationException(string message) : InvalidOperationException(message);
