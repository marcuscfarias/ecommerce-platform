using System.Text.Json;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Ecommerce.Kernel.API.Security;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.IntegrationTests.Auth.Login;

public sealed class LoginIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/auth/login";
    private const string ValidEmail = "user@example.com";
    private const string ValidPassword = "TestPassword1!";

    // Pre-computed at low cost so seeded hashes verify quickly during tests.
    private static readonly string ValidPasswordHash =
        BCrypt.Net.BCrypt.HashPassword(ValidPassword, 4);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed record LoginResponseBody(int Id, string Email, string Name);

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldSetCookiesAndReturnUserSummary()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Users.Add(new User(ValidEmail, ValidPasswordHash, "Jane Doe", isActive: true));
            return Task.CompletedTask;
        });
        var request = new { email = ValidEmail, password = ValidPassword };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var raw = await response.Content.ReadAsStringAsync();
        raw.ShouldNotContain("token", Case.Insensitive); // no access/refresh token leaked into the body

        var body = JsonSerializer.Deserialize<LoginResponseBody>(raw, JsonOptions);
        body.ShouldNotBeNull();
        body.Id.ShouldBeGreaterThan(0);
        body.Email.ShouldBe(ValidEmail);
        body.Name.ShouldBe("Jane Doe");

        var accessCookie = GetSetCookie(response, AuthCookieNames.AccessToken).ToLowerInvariant();
        accessCookie.ShouldContain("httponly");
        accessCookie.ShouldContain("secure");
        accessCookie.ShouldContain("samesite=strict");
        accessCookie.ShouldContain("path=/");

        var refreshCookie = GetSetCookie(response, AuthCookieNames.RefreshToken).ToLowerInvariant();
        refreshCookie.ShouldContain("httponly");
        refreshCookie.ShouldContain("secure");
        refreshCookie.ShouldContain("samesite=strict");
        refreshCookie.ShouldContain("path=/api/v1/auth");
    }

    [Fact]
    public async Task Login_WhenEmailDoesNotExist_ShouldReturnUnauthorized()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new { email = "ghost@example.com", password = ValidPassword };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Invalid credentials");
    }

    [Fact]
    public async Task Login_WhenPasswordIsWrong_ShouldReturnUnauthorized()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Users.Add(new User(ValidEmail, ValidPasswordHash, "Jane Doe", isActive: true));
            return Task.CompletedTask;
        });
        var request = new { email = ValidEmail, password = "WrongPassword1!" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Invalid credentials");
    }

    [Fact]
    public async Task Login_WhenUserIsInactive_ShouldReturnUnauthorized()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Users.Add(new User(ValidEmail, ValidPasswordHash, "Jane Doe", isActive: false));
            return Task.CompletedTask;
        });
        var request = new { email = ValidEmail, password = ValidPassword };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Invalid credentials");
    }

    [Fact]
    public async Task Login_WhenAntiEnumResponsesCompared_ShouldBeIdentical()
    {
        await ResetDatabaseAsync();

        // Arrange — seed two users: one active (for wrong-password path) and one inactive.
        await SeedAsync(db =>
        {
            db.Users.Add(new User("active@example.com", ValidPasswordHash, "Active User", isActive: true));
            db.Users.Add(new User("inactive@example.com", ValidPasswordHash, "Inactive User", isActive: false));
            return Task.CompletedTask;
        });

        // Act — three 401 paths that must look identical to a caller.
        var nonexistent = await Client.PostAsJsonAsync(Endpoint,
            new { email = "ghost@example.com", password = ValidPassword });
        var wrongPassword = await Client.PostAsJsonAsync(Endpoint,
            new { email = "active@example.com", password = "WrongPassword1!" });
        var inactive = await Client.PostAsJsonAsync(Endpoint,
            new { email = "inactive@example.com", password = ValidPassword });

        // Assert — status, content-type and the semantic body fields must all match.
        nonexistent.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        wrongPassword.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        inactive.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var nonexistentBody = await nonexistent.Content.ReadFromJsonAsync<ProblemDetails>();
        var wrongPasswordBody = await wrongPassword.Content.ReadFromJsonAsync<ProblemDetails>();
        var inactiveBody = await inactive.Content.ReadFromJsonAsync<ProblemDetails>();

        nonexistentBody.ShouldNotBeNull();
        wrongPasswordBody.ShouldNotBeNull();
        inactiveBody.ShouldNotBeNull();

        wrongPasswordBody.Status.ShouldBe(nonexistentBody.Status);
        inactiveBody.Status.ShouldBe(nonexistentBody.Status);
        wrongPasswordBody.Title.ShouldBe(nonexistentBody.Title);
        inactiveBody.Title.ShouldBe(nonexistentBody.Title);
        wrongPasswordBody.Detail.ShouldBe(nonexistentBody.Detail);
        inactiveBody.Detail.ShouldBe(nonexistentBody.Detail);

        wrongPassword.Content.Headers.ContentType?.MediaType
            .ShouldBe(nonexistent.Content.Headers.ContentType?.MediaType);
        inactive.Content.Headers.ContentType?.MediaType
            .ShouldBe(nonexistent.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Login_WhenEmailIsMissing_ShouldReturnValidationProblem()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new { email = "", password = ValidPassword };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Email");
    }

    [Fact]
    public async Task Login_WhenPasswordIsMissing_ShouldReturnValidationProblem()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new { email = ValidEmail, password = "" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Password");
    }

    private static string GetSetCookie(HttpResponseMessage response, string cookieName)
    {
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values
            : [];

        return setCookies.Single(c => c.StartsWith($"{cookieName}=", StringComparison.Ordinal));
    }
}
