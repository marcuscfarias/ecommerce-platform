using Ecommerce.Auth.IntegrationTests.Base.RateLimited;

namespace Ecommerce.Auth.IntegrationTests.Auth;

[Collection(nameof(RateLimitedAuthTestCollection))]
public sealed class EmailDispatchRateLimitIntegrationTests(RateLimitedAuthIntegrationFixture fixture)
{
    private const string RegisterEndpoint = "/api/v1/auth/register";
    private const string ResendEndpoint = "/api/v1/auth/resend-confirmation";
    private readonly HttpClient _client = fixture.Client;

    [Fact]
    public async Task RegisterAndResend_WhenCombinedRequestsExceedEmailDispatchLimit_ShouldReturnTooManyRequests()
    {
        // Arrange
        var register = new { name = "Rate Limit", email = "ratelimit-register@example.com", password = "Str0ngPass1" };
        var resend = new { email = "ratelimit-resend@example.com" };

        // Act — the limiter allows two combined requests per window; every later one is throttled.
        var firstRegister = await _client.PostAsJsonAsync(RegisterEndpoint, register);
        var firstResend = await _client.PostAsJsonAsync(ResendEndpoint, resend);
        var thirdRegister = await _client.PostAsJsonAsync(RegisterEndpoint, register);
        var thirdResend = await _client.PostAsJsonAsync(ResendEndpoint, resend);

        // Assert
        firstRegister.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        firstResend.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        thirdRegister.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        thirdRegister.Headers.RetryAfter.ShouldNotBeNull().Delta.ShouldNotBeNull();
        thirdResend.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        thirdResend.Headers.RetryAfter.ShouldNotBeNull().Delta.ShouldNotBeNull();
    }
}
