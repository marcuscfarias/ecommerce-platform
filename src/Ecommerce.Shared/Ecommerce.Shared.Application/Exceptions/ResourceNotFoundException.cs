using Ecommerce.Shared.Domain.Exceptions;

namespace Ecommerce.Shared.Application.Exceptions;

public class ResourceNotFoundException(string resourceName, int id)
    : Exception($"{resourceName} with Id {id} couldn't be found."), IAppException
{
    public int StatusCode => 404;
}
