using Ecommerce.Auth.Domain.Enums;

namespace Ecommerce.Auth.Application.Auth.Security;

public interface IJwtTokenGenerator
{
    JwtAccessToken Generate(int userId, string email, string name, IEnumerable<RoleName> roles);
}
