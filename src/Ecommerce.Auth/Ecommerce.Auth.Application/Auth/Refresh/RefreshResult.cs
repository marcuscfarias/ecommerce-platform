namespace Ecommerce.Auth.Application.Auth.Refresh;

public sealed record RefreshResult(string AccessToken, int AccessTokenExpiresInSeconds);
