using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Auth.Application.Exceptions;

public sealed class InvalidCredentialsException()
    : Exception("Invalid credentials"), IExceptionContract
{
    public int StatusCode => 401;
}
