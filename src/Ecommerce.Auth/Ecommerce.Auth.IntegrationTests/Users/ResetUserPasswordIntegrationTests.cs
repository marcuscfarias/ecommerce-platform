using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Auth.IntegrationTests.Users;

public sealed class ResetUserPasswordIntegrationTests : BaseAuthIntegrationTest
{
    private const string Endpoint = "/api/v1/users";
    private readonly HttpClient _client;

    public ResetUserPasswordIntegrationTests(AuthIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(AuthPermissions.ManageUsers);

    [Fact]
    public async Task ResetPassword_WhenPasswordIsValid_ShouldReturn204RehashRotateStampAndRevokeTokens()
    {
        await ResetDatabaseAsync();

        // Arrange — an active user with a live refresh token.
        var user = new User("jane@example.com", "old-hash", "Jane Doe");
        await SeedAsync(async db =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.RefreshTokens.Add(new RefreshToken(
                user.Id, "token-hash", DateTimeOffset.UtcNow.AddDays(7), user.SecurityStamp, DateTimeOffset.UtcNow));
        });
        var originalStamp = user.SecurityStamp;

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{user.Id}/password", new { Password = "N3wStr0ngPass" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        User? storedUser = null;
        RefreshToken? storedToken = null;
        await SeedAsync(async db =>
        {
            storedUser = await db.Users.SingleAsync(u => u.Id == user.Id);
            storedToken = await db.RefreshTokens.SingleAsync(t => t.UserId == user.Id);
        });
        storedUser!.PasswordHash.ShouldNotBe("old-hash");
        storedUser.SecurityStamp.ShouldNotBe(originalStamp);
        storedToken!.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task ResetPassword_WhenPasswordIsWeak_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var user = new User("jane@example.com", "hash", "Jane Doe");
        await SeedAsync(db =>
        {
            db.Users.Add(user);
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{user.Id}/password", new { Password = "weak" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Password");
    }

    [Fact]
    public async Task ResetPassword_WhenUserDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/999999/password", new { Password = "N3wStr0ngPass" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task ResetPassword_WhenTargetIsAdmin_ShouldReturn409WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange — an admin user must not be modified.
        var admin = new User("admin@example.com", "hash", "Admin User");
        await SeedAsync(db =>
        {
            var adminRole = new Role("Admin");
            db.Roles.Add(adminRole);
            db.Users.Add(admin);
            admin.AssignRole(adminRole);
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{admin.Id}/password", new { Password = "N3wStr0ngPass" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
