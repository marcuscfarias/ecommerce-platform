using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Products;

public sealed class UpdateProductIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/products";
    private readonly HttpClient _client;

    public UpdateProductIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.Manage);

    [Fact]
    public async Task Put_WhenRequestIsValid_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var product = new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", category.Id, 50);
        await SeedAsync(db =>
        {
            db.Products.Add(product);
            return Task.CompletedTask;
        });

        var request = new
        {
            name = "Mechanical Keyboard v2",
            description = "Now with gasket mount.",
            price = 139.99m,
            sku = "KB-75-HS",
            categoryId = category.Id,
            stockQuantity = 80
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{product.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Put_WhenIdDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var request = new
        {
            name = "Ghost Product",
            price = 19.99m,
            sku = "GHOST-1",
            categoryId = category.Id,
            stockQuantity = 1
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/999999", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Put_WhenCategoryDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var product = new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", category.Id, 50);
        await SeedAsync(db =>
        {
            db.Products.Add(product);
            return Task.CompletedTask;
        });

        var request = new
        {
            name = "Mechanical Keyboard",
            price = 129.99m,
            sku = "KB-75-HS",
            categoryId = 999999,
            stockQuantity = 50
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{product.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Put_WhenSkuBelongsToAnotherProduct_ShouldReturn409WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var target = new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", category.Id, 50);
        var other = new Product("Wireless Mouse", null, new Money(49.90m), "MS-WL-1", category.Id, 20);
        await SeedAsync(db =>
        {
            db.Products.Add(target);
            db.Products.Add(other);
            return Task.CompletedTask;
        });

        var request = new
        {
            name = "Mechanical Keyboard",
            price = 129.99m,
            sku = "MS-WL-1",
            categoryId = category.Id,
            stockQuantity = 50
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{target.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Put_WhenNameIsEmpty_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var product = new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", category.Id, 50);
        await SeedAsync(db =>
        {
            db.Products.Add(product);
            return Task.CompletedTask;
        });

        var request = new
        {
            name = "",
            price = 129.99m,
            sku = "KB-75-HS",
            categoryId = category.Id,
            stockQuantity = 50
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{product.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }
}
