using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Products;

public sealed class CreateProductIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/products";
    private readonly HttpClient _client;

    public CreateProductIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.Manage);

    [Fact]
    public async Task Post_WhenRequestIsValid_ShouldReturn201WithLocationHeader()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", "Electronic devices and accessories");
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var request = new
        {
            name = "Mechanical Keyboard",
            description = "A hot-swappable 75% mechanical keyboard.",
            price = 129.99m,
            sku = "KB-75-HS",
            categoryId = category.Id,
            stockQuantity = 50
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        var location = response.Headers.Location!.ToString();
        location.ShouldContain("/api/v1/products/");

        var idSegment = location[(location.LastIndexOf('/') + 1)..];
        int.TryParse(idSegment, out var id).ShouldBeTrue();
        id.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Post_WhenSkuAlreadyExists_ShouldReturn409WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", "Electronic devices and accessories");
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var existing = new Product("Existing Product", null, new Money(10m), "DUP-SKU-1", category.Id, 5);
        await SeedAsync(db =>
        {
            db.Products.Add(existing);
            return Task.CompletedTask;
        });

        var request = new
        {
            name = "Another Product",
            price = 49.99m,
            sku = "DUP-SKU-1",
            categoryId = category.Id,
            stockQuantity = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Post_WhenCategoryDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new
        {
            name = "Orphan Product",
            price = 19.99m,
            sku = "ORPHAN-1",
            categoryId = 999999,
            stockQuantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
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
            name = "",
            price = 19.99m,
            sku = "VALID-SKU",
            categoryId = 1,
            stockQuantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }
}
