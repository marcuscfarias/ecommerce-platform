using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class UpdateCategoryIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/categories";

    [Fact]
    public async Task Put_WhenRequestIsValid_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new Category("Electronics", "electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(seeded);
            return Task.CompletedTask;
        });
        var request = new
        {
            name = "Consumer Electronics",
            slug = "consumer-electronics",
            description = "Updated description text",
            isActive = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/{seeded.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Put_WhenRequestIsInvalid_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new Category("Electronics", "electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(seeded);
            return Task.CompletedTask;
        });
        var request = new
        {
            name = "Valid Name",
            slug = "",
            description = (string?)null,
            isActive = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/{seeded.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Put_WhenIdDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new
        {
            name = "Electronics",
            slug = "electronics",
            description = (string?)null,
            isActive = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/999999", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Put_WhenSlugBelongsToAnotherCategory_ShouldReturn409WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var electronics = new Category("Electronics", "electronics", null);
        var books = new Category("Books", "books", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(electronics);
            db.Categories.Add(books);
            return Task.CompletedTask;
        });
        var request = new
        {
            name = "Books",
            slug = "electronics",
            description = (string?)null,
            isActive = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/{books.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
