using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Auth.IntegrationTests.Users;

public sealed class SetUserRolesIntegrationTests : BaseAuthIntegrationTest
{
    private const string Endpoint = "/api/v1/users";
    private readonly HttpClient _client;

    public SetUserRolesIntegrationTests(AuthIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(AuthPermissions.ManageUsers);

    [Fact]
    public async Task SetRoles_WhenRolesAreValid_ShouldReplaceUserRolesAndReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange — a Manager user; Owner role available to assign.
        var user = new User("jane@example.com", "hash", "Jane Doe");
        await SeedAsync(async db =>
        {
            var manager = new Role(nameof(RoleName.Manager));
            db.Roles.AddRange(manager, new Role(nameof(RoleName.Owner)));
            await db.SaveChangesAsync();
            user.AssignRole(manager);
            db.Users.Add(user);
        });

        // Act
        string[] roles = ["Owner"];
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{user.Id}/roles", new { Roles = roles });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        User? stored = null;
        await SeedAsync(async db => stored = await db.Users.Include(u => u.Roles).SingleAsync(u => u.Id == user.Id));
        stored!.Roles.Count.ShouldBe(1);
        stored.Roles.ShouldContain(r => r.Name == "Owner");
    }

    [Fact]
    public async Task SetRoles_WhenRoleNameIsUnknown_ShouldReturn400WithValidationProblemDetails()
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
        string[] roles = ["Wizard"];
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{user.Id}/roles", new { Roles = roles });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.Keys.ShouldContain(k => k.StartsWith("Roles"));
    }

    [Fact]
    public async Task SetRoles_WhenUserDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        string[] roles = ["Owner"];
        var response = await _client.PutAsJsonAsync($"{Endpoint}/999999/roles", new { Roles = roles });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task SetRoles_WhenTargetIsAdmin_ShouldReturn409WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange — an admin user must not be modified.
        var admin = new User("admin@example.com", "hash", "Admin User");
        await SeedAsync(db =>
        {
            var adminRole = new Role(nameof(RoleName.Admin));
            db.Roles.Add(adminRole);
            db.Users.Add(admin);
            admin.AssignRole(adminRole);
            return Task.CompletedTask;
        });

        // Act
        string[] roles = ["Manager"];
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{admin.Id}/roles", new { Roles = roles });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task SetRoles_WhenTheRoleIsAdmin_ShouldReturn409WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var manager = new User("manager@example.com", "hash", "Manager User");
        await SeedAsync(db =>
        {
            var adminRole = new Role(nameof(RoleName.Admin));
            var managerRole = new Role(nameof(RoleName.Manager));
            db.Roles.Add(adminRole);
            db.Roles.Add(managerRole);
            db.Users.Add(manager);
            manager.AssignRole(managerRole);

            return Task.CompletedTask;
        });

        // Act
        string[] roles = ["Admin"];
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{manager.Id}/roles", new { Roles = roles });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
