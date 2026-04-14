namespace Ecommerce.Shared.Domain.Exceptions;

public interface IAppException
{
    int StatusCode { get; }
}
