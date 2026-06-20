using System.Net.Http.Headers;
using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Products;

public sealed class UploadProductImageIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/products";
    private readonly HttpClient _client;

    public UploadProductImageIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.Manage);

    [Fact]
    public async Task Post_WhenImageIsValid_ShouldReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var product = await SeedProductAsync();
        using var form = ImageForm(new byte[256], "image/jpeg", "keyboard.jpg");

        // Act
        var response = await _client.PostAsync($"{Endpoint}/{product.Id}/image", form);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Post_WhenProductAlreadyHasImage_ShouldReplaceAndReturn204()
    {
        await ResetDatabaseAsync();

        // Arrange
        var product = await SeedProductAsync(imageKey: "old.jpg");
        using var form = ImageForm(new byte[256], "image/png", "keyboard.png");

        // Act
        var response = await _client.PostAsync($"{Endpoint}/{product.Id}/image", form);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Post_WhenContentTypeIsUnsupported_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var product = await SeedProductAsync();
        using var form = ImageForm(new byte[256], "application/pdf", "keyboard.pdf");

        // Act
        var response = await _client.PostAsync($"{Endpoint}/{product.Id}/image", form);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Post_WhenFileExceedsMaxSize_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var product = await SeedProductAsync();
        using var form = ImageForm(new byte[(2 * 1024 * 1024) + 1], "image/jpeg", "huge.jpg");

        // Act
        var response = await _client.PostAsync($"{Endpoint}/{product.Id}/image", form);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Post_WhenProductDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        using var form = ImageForm(new byte[256], "image/jpeg", "keyboard.jpg");

        // Act
        var response = await _client.PostAsync($"{Endpoint}/999999/image", form);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    private async Task<Product> SeedProductAsync(string? imageKey = null)
    {
        var category = new Category("Electronics", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var product = new Product("Mechanical Keyboard", null, new Money(129.99m), "KB-75-HS", category.Id, 50);
        if (imageKey is not null)
            product.SetImageKey(imageKey);

        await SeedAsync(db =>
        {
            db.Products.Add(product);
            return Task.CompletedTask;
        });

        return product;
    }

    private static MultipartFormDataContent ImageForm(byte[] bytes, string contentType, string fileName)
    {
        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);
        return form;
    }
}
