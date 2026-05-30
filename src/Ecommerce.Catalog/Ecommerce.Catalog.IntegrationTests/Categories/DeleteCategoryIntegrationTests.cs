using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class DeleteCategoryIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/categories";
    private readonly HttpClient _client;

    public DeleteCategoryIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.Manage);

    [Fact]
    public async Task Delete_WhenIdExists_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var seeded = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(seeded);
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.DeleteAsync($"{Endpoint}/{seeded.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WhenIdDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.DeleteAsync($"{Endpoint}/999999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
