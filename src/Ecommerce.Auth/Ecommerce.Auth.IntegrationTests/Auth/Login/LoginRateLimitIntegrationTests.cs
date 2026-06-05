using Ecommerce.Auth.IntegrationTests.Base;

namespace Ecommerce.Auth.IntegrationTests.Auth.Login;

[Collection(nameof(RateLimitedAuthTestCollection))]
public sealed class LoginRateLimitIntegrationTests(RateLimitedAuthIntegrationFixture fixture)
{
    private const string Endpoint = "/api/v1/auth/login";
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task Login_WhenRequestsExceedLoginLimit_ShouldReturnTooManyRequests()
    {
        // Arrange
        var request = new { email = "ratelimit@example.com", password = "WhateverP@ss1" };

        // Act — the limiter allows two requests per window; the third is throttled.
        var first = await _client.PostAsJsonAsync(Endpoint, request);
        var second = await _client.PostAsJsonAsync(Endpoint, request);
        var third = await _client.PostAsJsonAsync(Endpoint, request);

        // Assert
        first.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        second.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
        third.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        third.Headers.RetryAfter.ShouldNotBeNull();
    }
}
