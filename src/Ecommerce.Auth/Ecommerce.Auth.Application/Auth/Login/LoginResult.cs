namespace Ecommerce.Auth.Application.Auth.Login;

public sealed record LoginResult(string AccessToken, string TokenType, int ExpiresInSeconds);
