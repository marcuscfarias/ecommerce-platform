namespace Ecommerce.Auth.Application.Users.Security;

public sealed record JwtAccessToken(string Token, int ExpiresInSeconds);
