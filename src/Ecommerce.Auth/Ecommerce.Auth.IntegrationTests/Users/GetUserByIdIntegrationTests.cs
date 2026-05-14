using Ecommerce.Auth.Api.Users.GetUserById;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.IntegrationTests.Users;

public sealed class GetUserByIdIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/users";

    [Fact]
    public async Task Get_WhenIdExists_ShouldReturn200WithUser()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new User("jane@example.com", "hash", "Jane", "Doe");
        await SeedAsync(db =>
        {
            db.Users.Add(seeded);
            return Task.CompletedTask;
        });

        // Act
        var response = await Client.GetAsync($"{Endpoint}/{seeded.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetUserByIdResponse>();
        body.ShouldNotBeNull();
        body.Id.ShouldBe(seeded.Id);
        body.Email.ShouldBe("jane@example.com");
        body.FirstName.ShouldBe("Jane");
        body.LastName.ShouldBe("Doe");
        body.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Get_WhenIdDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}/999999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
