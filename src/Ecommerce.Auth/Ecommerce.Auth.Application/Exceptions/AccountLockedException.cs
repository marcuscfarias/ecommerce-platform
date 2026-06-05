using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Auth.Application.Exceptions;

public sealed class AccountLockedException(int retryAfterSeconds)
    : Exception("Invalid credentials"), IExceptionContract, IRetryAfter
{
    public int StatusCode => 401;

    public int RetryAfterSeconds => retryAfterSeconds;
}
