using Ecommerce.Auth.Application.Auth.Logout;

namespace Ecommerce.Auth.Api.Auth.Logout;

public sealed record LogoutRequest
{
    internal static LogoutCommand ToCommand(string refreshToken) => new(refreshToken);
}
