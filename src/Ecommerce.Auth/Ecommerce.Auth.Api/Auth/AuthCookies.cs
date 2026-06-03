using Ecommerce.Auth.Application.Auth.Login;
using Ecommerce.Kernel.API.Security;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Auth.Api.Auth;

internal static class AuthCookies
{
    private const string AccessCookiePath = "/";
    private const string RefreshCookiePath = "/api/v1/auth";

    public static void SetAuthCookies(this HttpResponse response, AuthTokens tokens)
    {
        Append(response, AuthCookieNames.AccessToken, tokens.AccessToken, AccessCookiePath, tokens.AccessTokenExpiresInSeconds);
        Append(response, AuthCookieNames.RefreshToken, tokens.RefreshToken, RefreshCookiePath, tokens.RefreshTokenExpiresInSeconds);
    }

    private static void Append(HttpResponse response, string name, string value, string path, int maxAgeSeconds) =>
        response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = path,
            MaxAge = TimeSpan.FromSeconds(maxAgeSeconds),
        });
}
