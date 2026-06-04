using System.Security.Cryptography;
using System.Text;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Ecommerce.Kernel.API.Security;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.IntegrationTests.Auth;

public sealed class RefreshIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/auth/refresh";
    private const string PlainToken = "plain-refresh-token-value";

    [Fact]
    public async Task Refresh_WhenCookieIsValid_ShouldReturn204AndResetAccessCookie()
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

        var accessCookie = GetSetCookie(response, AuthCookieNames.AccessToken).ToLowerInvariant();
        accessCookie.ShouldContain("httponly");
        accessCookie.ShouldContain("secure");
        accessCookie.ShouldContain("samesite=strict");
        accessCookie.ShouldContain("path=/");
    }

    [Fact]
    public async Task Refresh_WhenCookieIsExpired_ShouldReturnUnauthorized()
    {
        await ResetDatabaseAsync();

        // Arrange
        var user = new User("jane@example.com", "hash", "Jane Doe", isActive: true);
        await SeedAsync(async db =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.RefreshTokens.Add(new RefreshToken(
                user.Id, HashOf(PlainToken), DateTimeOffset.UtcNow.AddDays(-1), user.SecurityStamp, DateTimeOffset.UtcNow.AddDays(-8)));
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Headers.Add("Cookie", $"{AuthCookieNames.RefreshToken}={PlainToken}");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Invalid refresh token");
    }

    [Fact]
    public async Task Refresh_WhenStampSnapshotMismatches_ShouldReturnUnauthorized()
    {
        await ResetDatabaseAsync();

        // Arrange — the snapshot differs from the user's current stamp (session invalidated).
        var user = new User("jane@example.com", "hash", "Jane Doe", isActive: true);
        await SeedAsync(async db =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.RefreshTokens.Add(new RefreshToken(
                user.Id, HashOf(PlainToken), DateTimeOffset.UtcNow.AddDays(7), "stale-stamp", DateTimeOffset.UtcNow));
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Headers.Add("Cookie", $"{AuthCookieNames.RefreshToken}={PlainToken}");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Invalid refresh token");
    }

    [Fact]
    public async Task Refresh_WhenNoCookie_ShouldReturnUnauthorized()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.PostAsync(Endpoint, content: null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Invalid refresh token");
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
