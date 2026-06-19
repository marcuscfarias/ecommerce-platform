using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Api.Products.ListProducts;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;

namespace Ecommerce.Catalog.IntegrationTests.Products;

public sealed class ListProductsIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/products";
    private readonly HttpClient _client;

    public ListProductsIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.View);

    [Fact]
    public async Task Get_WhenProductsExist_ShouldReturn200WithPagedResponse()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        await SeedAsync(db =>
        {
            db.Products.Add(new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", category.Id, 50));
            db.Products.Add(new Product("Wireless Mouse", null, new Money(49.90m), "MS-WL-1", category.Id, 20));
            db.Products.Add(new Product("USB-C Hub", null, new Money(39.00m), "HUB-USBC", category.Id, 10));
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListProductsResponse>();
        body.ShouldNotBeNull();
        body.Data.Count.ShouldBe(3);
        body.TotalCount.ShouldBe(3);
        body.Page.ShouldBe(1);
        body.TotalPages.ShouldBeGreaterThanOrEqualTo(1);

        var first = body.Data[0];
        first.Id.ShouldBeGreaterThan(0);
        first.Name.ShouldNotBeNullOrWhiteSpace();
        first.Price.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Get_WhenFilteringByCategoryId_ShouldReturnOnlyMatching()
    {
        await ResetDatabaseAsync();

        // Arrange
        var electronics = new Category("Electronics", null);
        var books = new Category("Books", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(electronics);
            db.Categories.Add(books);
            return Task.CompletedTask;
        });

        await SeedAsync(db =>
        {
            db.Products.Add(new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", electronics.Id, 50));
            db.Products.Add(new Product("Clean Code", null, new Money(35.00m), "BK-CC-1", books.Id, 100));
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.GetAsync($"{Endpoint}?categoryId={books.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListProductsResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBe(1);
        body.Data.Count.ShouldBe(1);
        body.Data[0].Name.ShouldBe("Clean Code");
    }

    [Fact]
    public async Task Get_WhenFilteringByIsActive_ShouldReturnOnlyMatching()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        await SeedAsync(db =>
        {
            db.Products.Add(new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", category.Id, 50));
            db.Products.Add(new Product("Discontinued Mouse", null, new Money(9.90m), "MS-OLD-1", category.Id, 0, isActive: false));
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.GetAsync($"{Endpoint}?isActive=false");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListProductsResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBe(1);
        body.Data.Count.ShouldBe(1);
        body.Data.ShouldAllBe(p => p.IsActive == false);
    }

    [Fact]
    public async Task Get_WhenNoProductsExist_ShouldReturn200WithEmptyResponse()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListProductsResponse>();
        body.ShouldNotBeNull();
        body.Data.ShouldBeEmpty();
        body.TotalCount.ShouldBe(0);
    }
}
