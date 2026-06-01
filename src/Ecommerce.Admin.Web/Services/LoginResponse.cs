namespace Ecommerce.Admin.Web.Services;

public sealed record LoginResponse(string AccessToken, string TokenType, int ExpiresInSeconds);
