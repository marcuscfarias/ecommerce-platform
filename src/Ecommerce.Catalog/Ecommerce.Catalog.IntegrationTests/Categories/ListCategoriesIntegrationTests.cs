using Ecommerce.Catalog.Api.Categories.ListCategories;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class ListCategoriesIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/categories";

    [Fact]
    public async Task Get_WhenCategoriesExist_ShouldReturn200WithPagedResponse()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Categories.Add(new Category("Electronics", "electronics", null));
            db.Categories.Add(new Category("Books", "books", null));
            db.Categories.Add(new Category("Clothing", "clothing", null));
            return Task.CompletedTask;
        });

        // Act
        var response = await Client.GetAsync(Endpoint);

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
        first.Slug.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Get_WhenFilteringByIsActive_ShouldReturnOnlyMatching()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Categories.Add(new Category("Electronics", "electronics", null));
            db.Categories.Add(new Category("Books", "books", null));
            db.Categories.Add(new Category("Clothing", "clothing", null, isActive: false));
            return Task.CompletedTask;
        });

        // Act
        var response = await Client.GetAsync($"{Endpoint}?isActive=false");

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
        var response = await Client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListCategoriesResponse>();
        body.ShouldNotBeNull();
        body.Data.ShouldBeEmpty();
        body.TotalCount.ShouldBe(0);
    }

    // [Fact]
    // public async Task Get_WhenPageNumberIsInvalid_ShouldReturn400WithValidationProblemDetails()
    // {
    //     await ResetDatabaseAsync();
    //
    //     // Act
    //     var response = await Client.GetAsync($"{Endpoint}?pageNumber=0");
    //
    //     // Assert
    //     response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    //     response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    //
    //     var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    //     body.ShouldNotBeNull();
    //     body.Errors.ShouldNotBeEmpty();
    // }
}
