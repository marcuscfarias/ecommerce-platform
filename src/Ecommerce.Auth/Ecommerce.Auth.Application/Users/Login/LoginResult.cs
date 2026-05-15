namespace Ecommerce.Auth.Application.Users.Login;

public sealed record LoginResult(string AccessToken, string TokenType, int ExpiresInSeconds);
