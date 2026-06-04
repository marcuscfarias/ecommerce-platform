using Ecommerce.Auth.Application.Auth.Login;

namespace Ecommerce.Auth.Api.Auth.Login;

public sealed record LoginResponse(int Id, string Email, string Name)
{
    internal static LoginResponse FromResult(LoginResult result) =>
        new(result.User.Id, result.User.Email, result.User.Name);
}
