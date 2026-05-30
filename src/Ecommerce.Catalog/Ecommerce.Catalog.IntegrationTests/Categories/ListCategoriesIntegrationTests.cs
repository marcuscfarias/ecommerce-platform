using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Api.Categories.ListCategories;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.IntegrationTests.Base;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class ListCategoriesIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/categories";
    private readonly HttpClient _client;

    public ListCategoriesIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.View);

    [Fact]
    public async Task Get_WhenCategoriesExist_ShouldReturn200WithPagedResponse()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Categories.Add(new Category("Electronics", null));
            db.Categories.Add(new Category("Books", null));
            db.Categories.Add(new Category("Clothing", null));
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListCategoriesResponse>();
        body.ShouldNotBeNull();
        body.Data.Count.ShouldBe(3);
        body.TotalCount.ShouldBe(3);
        body.Page.ShouldBe(1);
        body.TotalPages.ShouldBeGreaterThanOrEqualTo(1);

        var first = body.Data[0];
        first.Id.ShouldBeGreaterThan(0);
        first.Name.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Get_WhenFilteringByIsActive_ShouldReturnOnlyMatching()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Categories.Add(new Category("Electronics", null));
            db.Categories.Add(new Category("Books", null));
            db.Categories.Add(new Category("Clothing", null, isActive: false));
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.GetAsync($"{Endpoint}?isActive=false");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListCategoriesResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBe(1);
        body.Data.Count.ShouldBe(1);
        body.Data.ShouldAllBe(c => c.IsActive == false);
    }

    [Fact]
    public async Task Get_WhenNoCategoriesExist_ShouldReturn200WithEmptyResponse()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListCategoriesResponse>();
        body.ShouldNotBeNull();
        body.Data.ShouldBeEmpty();
        body.TotalCount.ShouldBe(0);
    }
}
