namespace Ecommerce.Auth.Application.Users.Security;

public interface IJwtTokenGenerator
{
    JwtAccessToken Generate(int userId, string email);
}
