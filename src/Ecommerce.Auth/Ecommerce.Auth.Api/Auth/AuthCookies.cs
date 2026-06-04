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
        response.SetAccessCookie(tokens.AccessToken, tokens.AccessTokenExpiresInSeconds);
        Append(response, AuthCookieNames.RefreshToken, tokens.RefreshToken, RefreshCookiePath, tokens.RefreshTokenExpiresInSeconds);
    }

    public static void SetAccessCookie(this HttpResponse response, string accessToken, int expiresInSeconds) =>
        Append(response, AuthCookieNames.AccessToken, accessToken, AccessCookiePath, expiresInSeconds);

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
