using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Auth.Application.Exceptions;

public sealed class EmailNotConfirmedException()
    : Exception("Email not confirmed"), IExceptionContract
{
    public int StatusCode => 403;
}
