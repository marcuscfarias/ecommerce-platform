using Ecommerce.Shared.Domain.Exceptions;

namespace Ecommerce.Shared.Domain.BusinessRules;

public class BusinessRuleValidationException(string message) : Exception(message), IAppException
{
    public int StatusCode => 409;
}
