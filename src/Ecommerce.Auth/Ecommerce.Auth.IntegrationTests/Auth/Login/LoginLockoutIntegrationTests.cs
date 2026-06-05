using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.IntegrationTests.Auth.Login;

public sealed class LoginLockoutIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/auth/login";
    private const string ValidEmail = "locked@example.com";
    private const string ValidPassword = "TestPassword1!";
    private const string WrongPassword = "WrongPassword1!";

    // Matches the default Auth:Lockout:MaxFailedAttempts so the lock trips on the fifth failure.
    private const int MaxFailedAttempts = 5;

    // Pre-computed at low cost so seeded hashes verify quickly during tests.
    private static readonly string ValidPasswordHash =
        BCrypt.Net.BCrypt.HashPassword(ValidPassword, 4);

    [Fact]
    public async Task Login_WhenCorrectPasswordDuringLockout_ShouldReturnUnauthorizedWithRetryAfter()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Users.Add(new User(ValidEmail, ValidPasswordHash, "Locked User", isActive: true));
            return Task.CompletedTask;
        });

        for (var attempt = 0; attempt < MaxFailedAttempts; attempt++)
            await Client.PostAsJsonAsync(Endpoint, new { email = ValidEmail, password = WrongPassword });

        // Act — the account is now locked; even the correct password must be rejected.
        var response = await Client.PostAsJsonAsync(Endpoint, new { email = ValidEmail, password = ValidPassword });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.RetryAfter.ShouldNotBeNull();
        response.Headers.RetryAfter.Delta!.Value.TotalSeconds.ShouldBeGreaterThan(0);

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Invalid credentials");
    }

    [Fact]
    public async Task Login_WhenSingleFailedAttempt_ShouldReturnUnauthorizedWithoutRetryAfter()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Users.Add(new User(ValidEmail, ValidPasswordHash, "Locked User", isActive: true));
            return Task.CompletedTask;
        });

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { email = ValidEmail, password = WrongPassword });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.RetryAfter.ShouldBeNull();

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Invalid credentials");
    }
}
