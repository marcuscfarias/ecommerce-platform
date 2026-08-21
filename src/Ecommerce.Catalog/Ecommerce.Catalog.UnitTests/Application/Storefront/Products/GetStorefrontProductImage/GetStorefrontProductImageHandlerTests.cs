using Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductImage;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Storefront.Products.GetStorefrontProductImage;

public class GetStorefrontProductImageHandlerTests
{
    private static readonly Faker Faker = new();
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly IProductImageStorage _imageStorage = Substitute.For<IProductImageStorage>();
    private readonly GetStorefrontProductImageHandler _handler;

    public GetStorefrontProductImageHandlerTests()
    {
        _handler = new GetStorefrontProductImageHandler(_repository, _imageStorage);
    }

    [Fact]
    public async Task Handle_WhenProductIsInactiveOrMissing_ShouldThrowResourceNotFound()
    {
        // Arrange
        var query = NewQuery();
        _repository.GetActiveByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductHasNoImage_ShouldThrowResourceNotFound()
    {
        // Arrange
        var query = NewQuery();
        _repository.GetActiveByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(NewProduct());

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenBlobIsMissing_ShouldThrowResourceNotFound()
    {
        // Arrange
        var query = NewQuery();
        var imageKey = NewImageKey();
        var product = NewProduct();
        product.SetImageKey(imageKey);
        _repository.GetActiveByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(product);
        _imageStorage.DownloadAsync(imageKey, Arg.Any<CancellationToken>()).Returns((ProductImageDownload?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenActiveProductHasImage_ShouldReturnContentContentTypeAndETag()
    {
        // Arrange
        var query = NewQuery();
        var imageKey = NewImageKey();
        var product = NewProduct();
        product.SetImageKey(imageKey);
        using var content = new MemoryStream(Faker.Random.Bytes(256));
        var contentType = Faker.System.MimeType();
        var contentLength = content.Length;
        var etag = Faker.Random.Guid().ToString();
        _repository.GetActiveByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(product);
        _imageStorage.DownloadAsync(imageKey, Arg.Any<CancellationToken>())
            .Returns(new ProductImageDownload(content, contentType, contentLength, etag));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Content.ShouldBe(content);
        result.ContentType.ShouldBe(contentType);
        result.ContentLength.ShouldBe(contentLength);
        result.ETag.ShouldBe(etag);
        await _repository.Received(1).GetActiveByIdAsync(query.Id, Arg.Any<CancellationToken>());
        await _imageStorage.Received(1).DownloadAsync(imageKey, Arg.Any<CancellationToken>());
    }

    private static GetStorefrontProductImageQuery NewQuery() => new(Faker.Random.Int(1, 1000));

    private static string NewImageKey() => $"{Faker.Random.Guid():N}.jpg";

    private static Product NewProduct() => new(
        Faker.Commerce.ProductName(),
        Faker.Lorem.Sentence(),
        new Money(Faker.Random.Decimal(1, 1000)),
        Faker.Commerce.Ean13(),
        Faker.Random.Int(1, 1000),
        Faker.Random.Int(0, 500));
}
