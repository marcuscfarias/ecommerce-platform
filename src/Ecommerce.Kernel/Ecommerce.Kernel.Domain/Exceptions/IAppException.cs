namespace Ecommerce.Kernel.Domain.Exceptions;

public interface IAppException
{
    int StatusCode { get; }
}
