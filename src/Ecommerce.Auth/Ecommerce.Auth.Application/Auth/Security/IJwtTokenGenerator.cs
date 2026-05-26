using Ecommerce.Auth.Domain.Enums;

namespace Ecommerce.Auth.Application.Auth.Security;

public interface IJwtTokenGenerator
{
    JwtAccessToken Generate(int userId, string email, IEnumerable<RoleName> roles);
}
