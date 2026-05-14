namespace Ecommerce.Auth.Application.Users.Security;

public interface IPasswordHasher
{
    string Hash(string plain);

    bool Verify(string plain, string hash);
}
