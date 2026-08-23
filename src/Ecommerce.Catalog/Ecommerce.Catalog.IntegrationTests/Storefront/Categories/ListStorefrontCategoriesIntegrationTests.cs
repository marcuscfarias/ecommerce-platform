using System.Text.Json;
using Ecommerce.Catalog.Api.Storefront.Categories.ListStorefrontCategories;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.IntegrationTests.Base;

namespace Ecommerce.Catalog.IntegrationTests.Storefront.Categories;

public sealed class ListStorefrontCategoriesIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/storefront/categories";

    [Fact]
    public async Task Get_WhenCategoriesExist_ShouldReturn200WithOnlyActiveCategories()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedCategoriesAsync(
            new Category("Electronics", null),
            new Category("Discontinued", null, isActive: false));

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<ListStorefrontCategoriesItemResponse>>();
        body.ShouldNotBeNull();
        body.Count.ShouldBe(1);
        body[0].Name.ShouldBe("Electronics");
    }

    [Fact]
    public async Task Get_WhenCategoriesExist_ShouldOrderByNameAscending()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedCategoriesAsync(
            new Category("Charlie", null),
            new Category("Alpha", null),
            new Category("Bravo", null));

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        var body = await response.Content.ReadFromJsonAsync<List<ListStorefrontCategoriesItemResponse>>();
        body.ShouldNotBeNull();
        body.Select(c => c.Name).ShouldBe(["Alpha", "Bravo", "Charlie"]);
    }

    [Fact]
    public async Task Get_WhenCategoriesExist_ShouldExposeOnlyThePublicFieldSet()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedCategoriesAsync(new Category("Electronics", null));

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var fields = payload.RootElement[0]
            .EnumerateObject()
            .Select(p => p.Name)
            .ToList();

        fields.ShouldBe(["id", "name"], ignoreOrder: true);
    }

    [Fact]
    public async Task Get_WhenNoActiveCategoryExists_ShouldReturn200WithEmptyArray()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<ListStorefrontCategoriesItemResponse>>();
        body.ShouldNotBeNull();
        body.ShouldBeEmpty();
    }

    private Task SeedCategoriesAsync(params Category[] categories) =>
        SeedAsync(db =>
        {
            db.Categories.AddRange(categories);
            return Task.CompletedTask;
        });
}
