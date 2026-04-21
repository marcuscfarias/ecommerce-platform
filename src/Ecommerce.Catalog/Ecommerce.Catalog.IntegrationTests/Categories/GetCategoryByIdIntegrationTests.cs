using Ecommerce.Catalog.Api.Categories.GetCategoryById;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class GetCategoryByIdIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/categories";

    [Fact]
    public async Task Get_WhenIdExists_ShouldReturn200WithCategory()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new Category("Electronics", "electronics", "Electronic devices and accessories");
        await SeedAsync(db =>
        {
            db.Categories.Add(seeded);
            return Task.CompletedTask;
        });

        // Act
        var response = await Client.GetAsync($"{Endpoint}/{seeded.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetCategoryByIdResponse>();
        body.ShouldNotBeNull();
        body.Id.ShouldBe(seeded.Id);
        body.Name.ShouldBe("Electronics");
        body.Slug.ShouldBe("electronics");
        body.Description.ShouldBe("Electronic devices and accessories");
        body.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Get_WhenIdDoesNotExist_ShouldReturn404WithProblemDetails()
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
}
