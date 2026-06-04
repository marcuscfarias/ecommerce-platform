using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Auth.Application.Exceptions;

public sealed class InvalidRefreshTokenException()
    : Exception("Invalid refresh token"), IExceptionContract
{
    public int StatusCode => 401;
}
