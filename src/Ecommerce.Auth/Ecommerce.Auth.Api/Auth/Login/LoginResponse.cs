using Ecommerce.Auth.Application.Users.Login;

namespace Ecommerce.Auth.Api.Auth.Login;

public sealed record LoginResponse(string AccessToken, string TokenType, int ExpiresInSeconds)
{
    internal static LoginResponse FromResult(LoginResult result) =>
        new(result.AccessToken, result.TokenType, result.ExpiresInSeconds);
}
