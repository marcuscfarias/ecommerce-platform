using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.IntegrationTests.Users;

public sealed class DeleteUserIntegrationTests : BaseAuthIntegrationTest
{
    private const string Endpoint = "/api/v1/users";
    private readonly HttpClient _client;

    public DeleteUserIntegrationTests(AuthIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(AuthPermissions.ManageUsers);

    [Fact]
    public async Task Delete_WhenUserExists_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new User("jane@example.com", "hash", "Jane Doe");
        await SeedAsync(db =>
        {
            db.Users.Add(seeded);
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.DeleteAsync($"{Endpoint}/{seeded.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.DeleteAsync($"{Endpoint}/999999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Delete_WhenTargetIsAdmin_ShouldReturn409WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange — an admin user must not be deactivable.
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
        var response = await _client.DeleteAsync($"{Endpoint}/{admin.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
