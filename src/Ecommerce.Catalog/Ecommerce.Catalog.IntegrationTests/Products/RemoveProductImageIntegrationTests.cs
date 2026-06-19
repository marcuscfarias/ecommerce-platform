using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Products;

public sealed class RemoveProductImageIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/products";
    private readonly HttpClient _client;

    public RemoveProductImageIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.Manage);

    [Fact]
    public async Task Delete_WhenProductHasImage_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var product = await SeedProductAsync(imageUrl: "http://127.0.0.1:10000/devstoreaccount1/product-images/keyboard.jpg");

        // Act
        var response = await _client.DeleteAsync($"{Endpoint}/{product.Id}/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WhenProductHasNoImage_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var product = await SeedProductAsync();

        // Act
        var response = await _client.DeleteAsync($"{Endpoint}/{product.Id}/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WhenProductDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.DeleteAsync($"{Endpoint}/999999/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    private async Task<Product> SeedProductAsync(string? imageUrl = null)
    {
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var product = new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", category.Id, 50);
        if (imageUrl is not null)
            product.SetImageUrl(imageUrl);

        await SeedAsync(db =>
        {
            db.Products.Add(product);
            return Task.CompletedTask;
        });

        return product;
    }
}
