namespace Ecommerce.Auth.Application.Auth.Security;

public interface IRefreshTokenFactory
{
    TimeSpan Lifetime { get; }

    RefreshTokenPair Create();

    string Hash(string plain);
}
