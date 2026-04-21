using Ecommerce.Shared.Domain.Exceptions;

namespace Ecommerce.Shared.UnitTests.API.Exceptions;

internal sealed class FakeAppException(int statusCode, string message)
    : Exception(message), IAppException
{
    public int StatusCode => statusCode;
}
