using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class UpdateCategoryIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/categories";
    private readonly HttpClient _client;

    public UpdateCategoryIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.Manage);

    [Fact]
    public async Task Put_WhenRequestIsValid_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(seeded);
            return Task.CompletedTask;
        });
        var request = new
        {
            name = "Consumer Electronics",
            description = "Updated description text"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{seeded.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Put_WhenNameIsEmpty_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(seeded);
            return Task.CompletedTask;
        });
        var request = new
        {
            name = "",
            description = (string?)null
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{seeded.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Put_WhenDescriptionIsTooShort_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(seeded);
            return Task.CompletedTask;
        });
        var request = new
        {
            name = "Electronics",
            description = "short"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{seeded.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Description");
    }

    [Fact]
    public async Task Put_WhenIdDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new
        {
            name = "Electronics",
            description = (string?)null
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/999999", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
