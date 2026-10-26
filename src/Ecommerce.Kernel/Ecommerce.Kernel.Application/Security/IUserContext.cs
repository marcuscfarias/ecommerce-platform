namespace Ecommerce.Kernel.Application.Security;

public interface IUserContext
{
    int UserId { get; }
    string Name { get; }
}
