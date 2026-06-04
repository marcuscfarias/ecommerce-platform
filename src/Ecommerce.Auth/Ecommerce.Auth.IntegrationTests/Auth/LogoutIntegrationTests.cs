using System.Security.Cryptography;
using System.Text;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Ecommerce.Kernel.API.Security;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Auth.IntegrationTests.Auth;

public sealed class LogoutIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/auth/logout";
    private const string PlainToken = "plain-refresh-token-value";

    [Fact]
    public async Task Logout_WhenCookieIsValid_ShouldReturn204RevokeTokenAndClearCookies()
    {
        await ResetDatabaseAsync();

        // Arrange
        var user = new User("jane@example.com", "hash", "Jane Doe", isActive: true);
        await SeedAsync(async db =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.RefreshTokens.Add(new RefreshToken(
                user.Id, HashOf(PlainToken), DateTimeOffset.UtcNow.AddDays(7), user.SecurityStamp, DateTimeOffset.UtcNow));
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Headers.Add("Cookie", $"{AuthCookieNames.RefreshToken}={PlainToken}");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        GetSetCookie(response, AuthCookieNames.AccessToken).ToLowerInvariant().ShouldContain("expires=thu, 01 jan 1970");
        GetSetCookie(response, AuthCookieNames.RefreshToken).ToLowerInvariant().ShouldContain("expires=thu, 01 jan 1970");

        RefreshToken? stored = null;
        await SeedAsync(async db =>
            stored = await db.RefreshTokens.SingleAsync(t => t.TokenHash == HashOf(PlainToken)));
        stored!.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Logout_WhenNoCookie_ShouldReturn204AndClearCookies()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.PostAsync(Endpoint, content: null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        GetSetCookie(response, AuthCookieNames.AccessToken).ToLowerInvariant().ShouldContain("expires=thu, 01 jan 1970");
        GetSetCookie(response, AuthCookieNames.RefreshToken).ToLowerInvariant().ShouldContain("expires=thu, 01 jan 1970");
    }

    private static string HashOf(string plain) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(plain)));

    private static string GetSetCookie(HttpResponseMessage response, string cookieName)
    {
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values
            : [];

        return setCookies.Single(c => c.StartsWith($"{cookieName}=", StringComparison.Ordinal));
    }
}
