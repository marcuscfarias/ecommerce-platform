using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class DeleteCategoryIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/categories";

    [Fact]
    public async Task Delete_WhenIdExists_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new Category("Electronics", "electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(seeded);
            return Task.CompletedTask;
        });

        // Act
        var response = await Client.DeleteAsync($"{Endpoint}/{seeded.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WhenIdDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.DeleteAsync($"{Endpoint}/999999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
