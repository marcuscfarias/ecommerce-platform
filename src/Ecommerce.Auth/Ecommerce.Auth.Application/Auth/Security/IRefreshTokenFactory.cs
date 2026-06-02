namespace Ecommerce.Auth.Application.Auth.Security;

public interface IRefreshTokenFactory
{
    RefreshTokenPair Create();

    string Hash(string plain);
}
