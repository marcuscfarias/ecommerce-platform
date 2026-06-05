using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Auth.IntegrationTests.Users;

public sealed class SetUserStatusIntegrationTests : BaseAuthIntegrationTest
{
    private const string Endpoint = "/api/v1/users";
    private readonly HttpClient _client;

    public SetUserStatusIntegrationTests(AuthIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(AuthPermissions.ManageUsers);

    [Fact]
    public async Task SetStatus_WhenDeactivatingActiveUser_ShouldReturn204AndRevokeRefreshTokens()
    {
        await ResetDatabaseAsync();

        // Arrange — an active user with a live refresh token.
        var user = new User("jane@example.com", "hash", "Jane Doe");
        await SeedAsync(async db =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            db.RefreshTokens.Add(new RefreshToken(
                user.Id, "token-hash", DateTimeOffset.UtcNow.AddDays(7), user.SecurityStamp, DateTimeOffset.UtcNow));
        });

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{user.Id}/status", new { IsActive = false });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        RefreshToken? stored = null;
        await SeedAsync(async db => stored = await db.RefreshTokens.SingleAsync(t => t.UserId == user.Id));
        stored!.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task SetStatus_WhenReactivatingInactiveUser_ShouldReturn204AndUserIsActive()
    {
        await ResetDatabaseAsync();

        // Arrange
        var user = new User("jane@example.com", "hash", "Jane Doe", isActive: false);
        await SeedAsync(db =>
        {
            db.Users.Add(user);
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{user.Id}/status", new { IsActive = true });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        User? stored = null;
        await SeedAsync(async db => stored = await db.Users.SingleAsync(u => u.Id == user.Id));
        stored!.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task SetStatus_WhenUserDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/999999/status", new { IsActive = false });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task SetStatus_WhenTargetIsAdmin_ShouldReturn409WithProblemDetails()
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
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{admin.Id}/status", new { IsActive = false });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
