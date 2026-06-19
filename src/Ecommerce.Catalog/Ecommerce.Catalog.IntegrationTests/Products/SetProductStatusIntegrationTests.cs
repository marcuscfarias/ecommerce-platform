using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Products;

public sealed class SetProductStatusIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/products";
    private readonly HttpClient _client;

    public SetProductStatusIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.Manage);

    [Fact]
    public async Task Put_WhenDeactivatingActiveProduct_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var product = new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", category.Id, 50);
        await SeedAsync(db =>
        {
            db.Products.Add(product);
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{product.Id}/status", new { isActive = false });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Put_WhenReactivatingInactiveProduct_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var product = new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", category.Id, 50, isActive: false);
        await SeedAsync(db =>
        {
            db.Products.Add(product);
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/{product.Id}/status", new { isActive = true });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Put_WhenIdDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.PutAsJsonAsync($"{Endpoint}/999999/status", new { isActive = false });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
