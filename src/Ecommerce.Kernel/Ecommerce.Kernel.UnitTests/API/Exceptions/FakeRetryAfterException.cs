using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Kernel.UnitTests.API.Exceptions;

internal sealed class FakeRetryAfterException(int statusCode, string message, int retryAfterSeconds)
    : Exception(message), IExceptionContract, IRetryAfter
{
    public int StatusCode => statusCode;

    public int RetryAfterSeconds => retryAfterSeconds;
}
