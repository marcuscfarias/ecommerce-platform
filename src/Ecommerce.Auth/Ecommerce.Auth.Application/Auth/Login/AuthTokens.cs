namespace Ecommerce.Auth.Application.Auth.Login;

public sealed record AuthTokens(
    string AccessToken,
    int AccessTokenExpiresInSeconds,
    string RefreshToken,
    int RefreshTokenExpiresInSeconds);
