namespace Ecommerce.Auth.Application.Auth.Security;

public interface IEmailConfirmationTokenFactory
{
    TimeSpan Lifetime { get; }

    EmailConfirmationTokenPair Create();

    string Hash(string plain);
}
