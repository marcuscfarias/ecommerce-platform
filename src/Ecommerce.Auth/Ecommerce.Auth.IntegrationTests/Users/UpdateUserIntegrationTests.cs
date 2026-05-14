using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.IntegrationTests.Users;

public sealed class UpdateUserIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/users";

    [Fact]
    public async Task Put_WhenUserExists_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new User("jane@example.com", "hash", "Jane", "Doe");
        await SeedAsync(db =>
        {
            db.Users.Add(seeded);
            return Task.CompletedTask;
        });
        var request = new { firstName = "Janet", lastName = "Smith", isActive = false };

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/{seeded.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Put_WhenUserDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new { firstName = "Janet", lastName = "Smith", isActive = true };

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/999999", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Put_WhenFirstNameIsEmpty_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new { firstName = "", lastName = "Smith", isActive = true };

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/1", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }
}
