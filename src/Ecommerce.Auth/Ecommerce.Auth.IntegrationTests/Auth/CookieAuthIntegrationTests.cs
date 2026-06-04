using System.Net.Http.Headers;
using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.IntegrationTests.Base;
using Ecommerce.Kernel.API.Security;
using Ecommerce.Kernel.IntegrationTests;

namespace Ecommerce.Auth.IntegrationTests.Auth;

// Guards the cookie fallback in the JWT bearer: a request authenticates from the
// access-token cookie, a tokenless Authorization header still falls back to the cookie,
// the Authorization header keeps precedence, and a request with neither is rejected.
public sealed class CookieAuthIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/users";

    [Fact]
    public async Task Get_WhenValidAccessTokenCookieAndNoHeader_ShouldReturn200()
    {
        await ResetDatabaseAsync();

        // Arrange
        var token = TestTokenFactory.Create([AuthPermissions.ViewUsers]);
        using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        request.Headers.Add("Cookie", $"{AuthCookieNames.AccessToken}={token}");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WhenTokenlessBearerHeaderAndValidAccessTokenCookie_ShouldReturn200()
    {
        await ResetDatabaseAsync();

        // Arrange
        var token = TestTokenFactory.Create([AuthPermissions.ViewUsers]);
        using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer");
        request.Headers.Add("Cookie", $"{AuthCookieNames.AccessToken}={token}");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WhenAuthorizationHeaderPresentWithInvalidCookie_ShouldReturn200()
    {
        await ResetDatabaseAsync();

        // Arrange
        var token = TestTokenFactory.Create([AuthPermissions.ViewUsers]);
        using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Cookie", $"{AuthCookieNames.AccessToken}=not-a-valid-token");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WhenNoCookieAndNoHeader_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
