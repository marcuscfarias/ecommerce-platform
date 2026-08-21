using System.Text.Json;
using Ecommerce.Catalog.Api.Storefront.Products.GetStorefrontProductById;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Storefront.Products;

public sealed class GetStorefrontProductByIdIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/storefront/products";

    [Fact]
    public async Task Get_WhenProductIsActive_ShouldReturn200WithTheExactFieldSet()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        var product = NewProduct(category.Id, name: "Green Tea Sampler", description: "Loose leaf tea assortment.");
        await SeedProductAsync(product);

        // Act
        var response = await Client.GetAsync($"{Endpoint}/{product.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var fields = payload.RootElement.EnumerateObject().Select(p => p.Name).ToList();
        fields.ShouldBe(["id", "name", "description", "price", "categoryId", "inStock", "hasImage"], ignoreOrder: true);

        var body = await response.Content.ReadFromJsonAsync<GetStorefrontProductByIdResponse>();
        body.ShouldNotBeNull();
        body.Id.ShouldBe(product.Id);
        body.Name.ShouldBe("Green Tea Sampler");
        body.Description.ShouldBe("Loose leaf tea assortment.");
        body.Price.ShouldBe(10.00m);
        body.CategoryId.ShouldBe(category.Id);
        body.InStock.ShouldBeTrue();
        body.HasImage.ShouldBeFalse();
    }

    [Fact]
    public async Task Get_WhenProductIsInactive_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        var product = NewProduct(category.Id, isActive: false);
        await SeedProductAsync(product);

        // Act
        var response = await Client.GetAsync($"{Endpoint}/{product.Id}");

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
        var response = await Client.GetAsync($"{Endpoint}/999999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    private async Task<Category> SeedCategoryAsync()
    {
        var category = new Category($"Category {Guid.NewGuid():N}", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        return category;
    }

    private Task SeedProductAsync(Product product) =>
        SeedAsync(db =>
        {
            db.Products.Add(product);
            return Task.CompletedTask;
        });

    private static Product NewProduct(
        int categoryId,
        string? name = null,
        decimal price = 10.00m,
        string? description = null,
        int stockQuantity = 5,
        bool isActive = true) =>
        new(name ?? $"Product {Guid.NewGuid():N}",
            description,
            new Money(price),
            $"SKU-{Guid.NewGuid():N}",
            categoryId,
            stockQuantity,
            isActive);
}
