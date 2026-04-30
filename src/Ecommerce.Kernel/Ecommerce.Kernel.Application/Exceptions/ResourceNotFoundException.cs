using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Kernel.Application.Exceptions;

public class ResourceNotFoundException(string resourceName, int id)
    : Exception($"{resourceName} with Id {id} couldn't be found."), IAppException
{
    public int StatusCode => 404;
}
