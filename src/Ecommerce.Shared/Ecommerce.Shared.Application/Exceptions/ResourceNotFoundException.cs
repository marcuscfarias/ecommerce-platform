using Ecommerce.Shared.Domain.Exceptions;

namespace Ecommerce.Shared.Application.Exceptions;

public class ResourceNotFoundException(string message) : Exception(message), IAppException
{
    public int StatusCode => 404;
    public string ErrorMessage => Message;
}
