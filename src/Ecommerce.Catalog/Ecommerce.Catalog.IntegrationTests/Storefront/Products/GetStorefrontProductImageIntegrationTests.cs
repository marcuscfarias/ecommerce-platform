using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Storefront.Products;

public sealed class GetStorefrontProductImageIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/storefront/products";

    [Fact]
    public async Task Get_WhenActiveProductHasImage_ShouldReturn200WithImageContent()
    {
        await ResetDatabaseAsync();

        // Arrange
        var content = NewImageContent();
        var imageKey = await UploadImageAsync(content, "image/jpeg");
        var product = await SeedProductAsync(imageKey: imageKey);

        // Act
        var response = await Client.GetAsync($"{Endpoint}/{product.Id}/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("image/jpeg");

        var body = await response.Content.ReadAsByteArrayAsync();
        body.ShouldBe(content);
    }

    [Fact]
    public async Task Get_WhenActiveProductHasImage_ShouldReturnPublicCacheControlAndEtag()
    {
        await ResetDatabaseAsync();

        // Arrange
        var imageKey = await UploadImageAsync(NewImageContent(), "image/jpeg");
        var product = await SeedProductAsync(imageKey: imageKey);

        // Act
        var response = await Client.GetAsync($"{Endpoint}/{product.Id}/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl.ShouldNotBeNull();
        response.Headers.CacheControl!.Public.ShouldBeTrue();
        response.Headers.CacheControl.MaxAge.ShouldBe(TimeSpan.FromSeconds(86400));
        response.Headers.ETag.ShouldNotBeNull();
        response.Headers.ETag!.Tag.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Get_WhenIfNoneMatchMatchesEtag_ShouldReturn304NotModified()
    {
        await ResetDatabaseAsync();

        // Arrange
        var imageKey = await UploadImageAsync(NewImageContent(), "image/jpeg");
        var product = await SeedProductAsync(imageKey: imageKey);
        var firstResponse = await Client.GetAsync($"{Endpoint}/{product.Id}/image");
        var etag = firstResponse.Headers.ETag!;

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{Endpoint}/{product.Id}/image");
        request.Headers.IfNoneMatch.Add(etag);
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotModified);

        var body = await response.Content.ReadAsByteArrayAsync();
        body.ShouldBeEmpty();
    }

    [Fact]
    public async Task Get_WhenProductIsInactive_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var imageKey = await UploadImageAsync(NewImageContent(), "image/jpeg");
        var product = await SeedProductAsync(imageKey: imageKey, isActive: false);

        // Act
        var response = await Client.GetAsync($"{Endpoint}/{product.Id}/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Get_WhenActiveProductHasNoImage_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var product = await SeedProductAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}/{product.Id}/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    [Fact]
    public async Task Get_WhenProductDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}/999999/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }

    private static byte[] NewImageContent()
    {
        var content = new byte[256];
        Random.Shared.NextBytes(content);
        return content;
    }

    private async Task<Product> SeedProductAsync(string? imageKey = null, bool isActive = true)
    {
        var category = new Category($"Category {Guid.NewGuid():N}", null);
        await SeedAsync(db =>
        {
            db.Categories.Add(category);
            return Task.CompletedTask;
        });

        var product = new Product(
            $"Product {Guid.NewGuid():N}",
            null,
            new Money(129.99m),
            $"SKU-{Guid.NewGuid():N}",
            category.Id,
            50,
            isActive);

        if (imageKey is not null)
        {
            product.SetImageKey(imageKey);
        }

        await SeedAsync(db =>
        {
            db.Products.Add(product);
            return Task.CompletedTask;
        });

        return product;
    }
}
