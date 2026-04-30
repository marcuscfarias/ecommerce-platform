using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Kernel.UnitTests.API.Exceptions;

internal sealed class FakeContractException(int statusCode, string message)
    : Exception(message), IExceptionContract
{
    public int StatusCode => statusCode;
}
