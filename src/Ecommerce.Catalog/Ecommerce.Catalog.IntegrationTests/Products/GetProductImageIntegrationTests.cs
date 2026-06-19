using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Products;

public sealed class GetProductImageIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/products";
    private readonly HttpClient _client;

    public GetProductImageIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.View);

    [Fact]
    public async Task Get_WhenProductHasImage_ShouldReturn200WithImageContent()
    {
        await ResetDatabaseAsync();

        // Arrange
        var content = new byte[256];
        Random.Shared.NextBytes(content);
        var imageUrl = await UploadImageAsync(content, "image/jpeg");
        var product = await SeedProductAsync(imageUrl);

        // Act
        var response = await _client.GetAsync($"{Endpoint}/{product.Id}/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("image/jpeg");

        var body = await response.Content.ReadAsByteArrayAsync();
        body.ShouldBe(content);
    }

    [Fact]
    public async Task Get_WhenProductHasNoImage_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var product = await SeedProductAsync();

        // Act
        var response = await _client.GetAsync($"{Endpoint}/{product.Id}/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Get_WhenProductDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.GetAsync($"{Endpoint}/999999/image");

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
