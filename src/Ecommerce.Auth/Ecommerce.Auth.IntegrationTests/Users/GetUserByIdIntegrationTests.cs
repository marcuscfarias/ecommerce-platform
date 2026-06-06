using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.Api.Users.GetUserById;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.IntegrationTests.Users;

public sealed class GetUserByIdIntegrationTests : BaseAuthIntegrationTest
{
    private const string Endpoint = "/api/v1/users";
    private readonly HttpClient _client;

    public GetUserByIdIntegrationTests(AuthIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(AuthPermissions.ViewUsers);

    [Fact]
    public async Task Get_WhenIdExists_ShouldReturn200WithUser()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new User("jane@example.com", "hash", "Jane Doe");
        await SeedAsync(async db =>
        {
            var manager = new Role(nameof(RoleName.Manager));
            db.Roles.Add(manager);
            await db.SaveChangesAsync();
            seeded.AssignRole(manager);
            db.Users.Add(seeded);
        });

        // Act
        var response = await _client.GetAsync($"{Endpoint}/{seeded.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetUserByIdResponse>();
        body.ShouldNotBeNull();
        body.Id.ShouldBe(seeded.Id);
        body.Email.ShouldBe("jane@example.com");
        body.Name.ShouldBe("Jane Doe");
        body.IsActive.ShouldBeTrue();
        body.Roles.ShouldBe(["Manager"]);
    }

    [Fact]
    public async Task Get_WhenIdDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.GetAsync($"{Endpoint}/999999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
