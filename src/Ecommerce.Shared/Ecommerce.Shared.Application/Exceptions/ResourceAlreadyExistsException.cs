using Ecommerce.Shared.Domain.Exceptions;

namespace Ecommerce.Shared.Application.Exceptions;

public class ResourceAlreadyExistsException(string message) : InvalidOperationException(message), IAppException
{
    public int StatusCode => 409;
    public string ErrorMessage => Message;
}
