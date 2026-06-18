using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Api.Products.GetProductById;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Products;

public sealed class GetProductByIdIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/products";
    private readonly HttpClient _client;

    public GetProductByIdIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.View);

    [Fact]
    public async Task Get_WhenIdExists_ShouldReturn200WithProduct()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", "Electronic devices and accessories");
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var product = new Product("Mechanical Keyboard", "Hot-swappable 75% layout.", new Money(129.99m), "KB-75-HS", category.Id, 50);
        await SeedAsync(db =>
        {
            db.Products.Add(product);
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.GetAsync($"{Endpoint}/{product.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetProductByIdResponse>();
        body.ShouldNotBeNull();
        body.Id.ShouldBe(product.Id);
        body.Name.ShouldBe("Mechanical Keyboard");
        body.Description.ShouldBe("Hot-swappable 75% layout.");
        body.Price.ShouldBe(129.99m);
        body.Currency.ShouldBe("USD");
        body.Sku.ShouldBe("KB-75-HS");
        body.CategoryId.ShouldBe(category.Id);
        body.StockQuantity.ShouldBe(50);
        body.IsActive.ShouldBeTrue();
        body.ImageUrl.ShouldBeNull();
    }

    [Fact]
    public async Task Get_WhenIdDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.GetAsync($"{Endpoint}/999999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
