using System.Text.Json;
using Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Storefront.Products;

public sealed class ListStorefrontProductsIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/storefront/products";

    [Fact]
    public async Task Get_WhenProductsExist_ShouldReturn200WithOnlyActiveProducts()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductsAsync(
            NewProduct(category.Id, name: "Listed Product"),
            NewProduct(category.Id, name: "Retired Product", isActive: false));

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBe(1);
        body.Data.Count.ShouldBe(1);
        body.Data[0].Name.ShouldBe("Listed Product");
    }

    [Fact]
    public async Task Get_WhenProductsExist_ShouldExposeOnlyThePublicFieldSet()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductsAsync(NewProduct(category.Id));

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var fields = payload.RootElement
            .GetProperty("data")[0]
            .EnumerateObject()
            .Select(p => p.Name)
            .ToList();

        fields.ShouldBe(["id", "name", "price", "inStock", "hasImage"], ignoreOrder: true);
    }

    [Fact]
    public async Task Get_WhenProductIsOutOfStock_ShouldReportTheDerivedFlags()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductsAsync(
            NewProduct(category.Id, name: "Available", stockQuantity: 3),
            NewProduct(category.Id, name: "Sold Out", stockQuantity: 0));

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();

        var available = body.Data.Single(p => p.Name == "Available");
        available.InStock.ShouldBeTrue();
        available.HasImage.ShouldBeFalse();

        var soldOut = body.Data.Single(p => p.Name == "Sold Out");
        soldOut.InStock.ShouldBeFalse();
    }

    [Fact]
    public async Task Get_WhenFilteringByCategoryId_ShouldReturnOnlyThatCategory()
    {
        await ResetDatabaseAsync();

        // Arrange
        var electronics = await SeedCategoryAsync();
        var books = await SeedCategoryAsync();
        await SeedProductsAsync(
            NewProduct(electronics.Id, name: "Keyboard"),
            NewProduct(books.Id, name: "Clean Code"));

        // Act
        var response = await Client.GetAsync($"{Endpoint}?categoryId={books.Id}");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBe(1);
        body.Data[0].Name.ShouldBe("Clean Code");
    }

    [Fact]
    public async Task Get_WhenCategoryIdMatchesNoCategory_ShouldReturn200WithEmptyPage()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductsAsync(NewProduct(category.Id));

        // Act
        var response = await Client.GetAsync($"{Endpoint}?categoryId={category.Id + 999}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.Data.ShouldBeEmpty();
        body.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Get_WhenSearchMatchesTheName_ShouldReturnOnlyMatchingProducts()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductsAsync(
            NewProduct(category.Id, name: "Green Tea Sampler"),
            NewProduct(category.Id, name: "Coffee Grinder"));

        // Act
        var response = await Client.GetAsync($"{Endpoint}?search=tea");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBe(1);
        body.Data[0].Name.ShouldBe("Green Tea Sampler");
    }

    [Fact]
    public async Task Get_WhenSearchMatchesTheDescription_ShouldReturnOnlyMatchingProducts()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductsAsync(
            NewProduct(category.Id, name: "Sampler Box", description: "Loose leaf tea assortment."),
            NewProduct(category.Id, name: "Coffee Grinder", description: null));

        // Act
        var response = await Client.GetAsync($"{Endpoint}?search=tea");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBe(1);
        body.Data[0].Name.ShouldBe("Sampler Box");
    }

    [Fact]
    public async Task Get_WhenSearchIsWhitespaceOnly_ShouldApplyNoTextFilter()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductsAsync(
            NewProduct(category.Id, name: "Green Tea Sampler"),
            NewProduct(category.Id, name: "Coffee Grinder"));

        // Act
        var response = await Client.GetAsync($"{Endpoint}?search=%20%20");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task Get_WhenSearchIsAWildcardCharacter_ShouldTreatItAsLiteralText()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductsAsync(
            NewProduct(category.Id, name: "Green Tea Sampler"),
            NewProduct(category.Id, name: "Coffee Grinder"));

        // Act
        var response = await Client.GetAsync($"{Endpoint}?search=%25");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Get_WhenFilteringByCategoryIdAndSearch_ShouldApplyBothFilters()
    {
        await ResetDatabaseAsync();

        // Arrange
        var drinks = await SeedCategoryAsync();
        var books = await SeedCategoryAsync();
        await SeedProductsAsync(
            NewProduct(drinks.Id, name: "Green Tea Sampler"),
            NewProduct(drinks.Id, name: "Coffee Grinder"),
            NewProduct(books.Id, name: "Tea Ceremony Handbook"));

        // Act
        var response = await Client.GetAsync($"{Endpoint}?categoryId={drinks.Id}&search=tea");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.TotalCount.ShouldBe(1);
        body.Data[0].Name.ShouldBe("Green Tea Sampler");
    }

    [Fact]
    public async Task Get_WhenSortIsOmitted_ShouldOrderByNameAscending()
    {
        await ResetDatabaseAsync();
        await SeedSortableProductsAsync();

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.Data.Select(p => p.Name).ShouldBe(["Alpha", "Bravo", "Charlie"]);
    }

    [Fact]
    public async Task Get_WhenSortIsNameAscending_ShouldOrderByNameAscending()
    {
        await ResetDatabaseAsync();
        await SeedSortableProductsAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}?sort=name_asc");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.Data.Select(p => p.Name).ShouldBe(["Alpha", "Bravo", "Charlie"]);
    }

    [Fact]
    public async Task Get_WhenSortIsPriceAscending_ShouldOrderByPriceAscending()
    {
        await ResetDatabaseAsync();
        await SeedSortableProductsAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}?sort=price_asc");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.Data.Select(p => p.Price).ShouldBe([10.00m, 20.00m, 30.00m]);
    }

    [Fact]
    public async Task Get_WhenSortIsPriceDescending_ShouldOrderByPriceDescending()
    {
        await ResetDatabaseAsync();
        await SeedSortableProductsAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}?sort=price_desc");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.Data.Select(p => p.Price).ShouldBe([30.00m, 20.00m, 10.00m]);
    }

    [Fact]
    public async Task Get_WhenSortIsNewest_ShouldOrderByMostRecentlyAddedFirst()
    {
        await ResetDatabaseAsync();
        await SeedSortableProductsAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}?sort=newest");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.Data.Select(p => p.Name).ShouldBe(["Alpha", "Charlie", "Bravo"]);
    }

    [Fact]
    public async Task Get_WhenPageNumberExceedsTotalPages_ShouldReturn200WithEmptyPage()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        await SeedProductsAsync(NewProduct(category.Id));

        // Act
        var response = await Client.GetAsync($"{Endpoint}?pageNumber=99");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListStorefrontProductsResponse>();
        body.ShouldNotBeNull();
        body.Data.ShouldBeEmpty();
        body.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task Get_WhenPagingProductsThatShareAPrice_ShouldNotRepeatIdsAcrossPages()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = await SeedCategoryAsync();
        var products = Enumerable.Range(1, 12)
            .Select(i => NewProduct(category.Id, name: $"Tied Product {i:D2}", price: 19.99m))
            .ToArray();
        await SeedProductsAsync(products);

        // Act
        var firstPage = await Client.GetFromJsonAsync<ListStorefrontProductsResponse>(
            $"{Endpoint}?sort=price_asc&pageNumber=1");
        var secondPage = await Client.GetFromJsonAsync<ListStorefrontProductsResponse>(
            $"{Endpoint}?sort=price_asc&pageNumber=2");

        // Assert
        firstPage.ShouldNotBeNull();
        secondPage.ShouldNotBeNull();

        var ids = firstPage.Data.Concat(secondPage.Data).Select(p => p.Id).ToList();
        ids.Count.ShouldBe(12);
        ids.Distinct().Count().ShouldBe(12);
    }

    [Fact]
    public async Task Get_WhenPageNumberIsLessThanOne_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}?pageNumber=0");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("PageNumber");
    }

    [Fact]
    public async Task Get_WhenSortIsUnknown_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}?sort=relevance");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Sort");
    }

    [Fact]
    public async Task Get_WhenSearchExceedsTheMaximumLength_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}?search={new string('a', 201)}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Search");
    }

    [Fact]
    public async Task Get_WhenCategoryIdIsNotAnInteger_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}?categoryId=abc");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
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

    private Task SeedProductsAsync(params Product[] products) =>
        SeedAsync(db =>
        {
            db.Products.AddRange(products);
            return Task.CompletedTask;
        });

    private async Task SeedSortableProductsAsync()
    {
        var category = await SeedCategoryAsync();

        // Inserted out of alphabetical order so name ordering cannot pass by insertion order,
        // with "Alpha" last so `newest` puts it first.
        await SeedProductsAsync(
            NewProduct(category.Id, name: "Bravo", price: 30.00m),
            NewProduct(category.Id, name: "Charlie", price: 20.00m),
            NewProduct(category.Id, name: "Alpha", price: 10.00m));
    }

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
