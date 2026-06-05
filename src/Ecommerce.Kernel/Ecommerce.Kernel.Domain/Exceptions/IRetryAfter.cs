namespace Ecommerce.Kernel.Domain.Exceptions;

public interface IRetryAfter
{
    int RetryAfterSeconds { get; }
}
