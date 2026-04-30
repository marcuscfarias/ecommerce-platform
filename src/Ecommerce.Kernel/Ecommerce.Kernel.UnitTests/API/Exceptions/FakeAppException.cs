using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Kernel.UnitTests.API.Exceptions;

internal sealed class FakeAppException(int statusCode, string message)
    : Exception(message), IAppException
{
    public int StatusCode => statusCode;
}
