namespace Ecommerce.Kernel.Domain.Exceptions;

public class BusinessRuleValidationException(string message) : InvalidOperationException(message), IExceptionContract
{
    public int StatusCode => 409;
}
