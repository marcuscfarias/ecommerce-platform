using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.IntegrationTests.Users;

public sealed class CreateUserIntegrationTests(AuthIntegrationFixture fixture)
    : BaseAuthIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/users";

    [Fact]
    public async Task Post_WhenRequestIsValid_ShouldReturn201WithLocationHeader()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new
        {
            email = "user@example.com",
            password = "Str0ngPass1",
            name = "Jane Doe"
        };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        var location = response.Headers.Location!.ToString();
        location.ShouldContain("/api/v1/users/");

        var idSegment = location[(location.LastIndexOf('/') + 1)..];
        int.TryParse(idSegment, out var id).ShouldBeTrue();
        id.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Post_WhenEmailIsInvalid_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new
        {
            email = "not-an-email",
            password = "Str0ngPass1",
            name = "Jane Doe"
        };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Post_WhenPasswordIsWeak_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new
        {
            email = "user@example.com",
            password = "weak",
            name = "Jane Doe"
        };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Post_WhenEmailAlreadyExists_ShouldReturn409WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Users.Add(new User("user@example.com", "hash", "Existing User"));
            return Task.CompletedTask;
        });
        var duplicate = new
        {
            email = "user@example.com",
            password = "Str0ngPass1",
            name = "Other Person"
        };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, duplicate);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Post_WhenNameIsEmpty_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new
        {
            email = "user@example.com",
            password = "Str0ngPass1",
            name = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Name");
    }
}
