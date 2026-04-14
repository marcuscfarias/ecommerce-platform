using Ecommerce.Shared.Domain.Exceptions;

namespace Ecommerce.Shared.Application.Exceptions;

public class ResourceAlreadyExistsException(string message) : Exception(message), IAppException
{
    public int StatusCode => 409;
}
