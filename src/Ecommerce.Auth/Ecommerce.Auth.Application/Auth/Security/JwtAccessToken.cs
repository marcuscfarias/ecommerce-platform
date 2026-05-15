namespace Ecommerce.Auth.Application.Auth.Security;

public sealed record JwtAccessToken(string Token, int ExpiresInSeconds);
