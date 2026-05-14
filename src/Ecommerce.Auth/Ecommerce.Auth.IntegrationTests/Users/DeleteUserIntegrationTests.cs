using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.IntegrationTests.Users;

public sealed class DeleteUserIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/users";

    [Fact]
    public async Task Delete_WhenUserExists_ShouldReturn204()
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
        var response = await Client.DeleteAsync($"{Endpoint}/{seeded.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.DeleteAsync($"{Endpoint}/999999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
