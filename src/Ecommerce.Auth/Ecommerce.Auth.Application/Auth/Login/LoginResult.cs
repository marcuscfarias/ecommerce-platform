namespace Ecommerce.Auth.Application.Auth.Login;

public sealed record LoginResult(AuthTokens Tokens, UserSummary User);
