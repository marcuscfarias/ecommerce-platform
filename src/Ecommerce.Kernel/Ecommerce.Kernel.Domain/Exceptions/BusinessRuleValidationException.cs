namespace Ecommerce.Kernel.Domain.Exceptions;

public class BusinessRuleValidationException(string message) : InvalidOperationException(message), IAppException
{
    public int StatusCode => 409;
}
