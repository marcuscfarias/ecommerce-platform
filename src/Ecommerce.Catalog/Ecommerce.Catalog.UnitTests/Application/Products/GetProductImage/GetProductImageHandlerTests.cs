using Ecommerce.Catalog.Application.Products.GetProductImage;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Products.GetProductImage;

public class GetProductImageHandlerTests
{
    private static readonly Faker Faker = new();
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly IProductImageStorage _imageStorage = Substitute.For<IProductImageStorage>();
    private readonly GetProductImageHandler _handler;

    public GetProductImageHandlerTests()
    {
        _handler = new GetProductImageHandler(_repository, _imageStorage);
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldThrowResourceNotFound()
    {
        // Arrange
        var query = NewQuery();
        _repository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns((Product?)null);

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
        var product = NewProduct();
        _repository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(product);

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
        var imageKey = $"{Faker.Random.Guid():N}.jpg";
        var product = NewProduct();
        product.SetImageKey(imageKey);
        _repository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(product);
        _imageStorage.DownloadAsync(imageKey, Arg.Any<CancellationToken>()).Returns((ProductImageDownload?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenImageExists_ShouldReturnContentAndContentType()
    {
        // Arrange
        var query = NewQuery();
        var imageKey = $"{Faker.Random.Guid():N}.jpg";
        var product = NewProduct();
        product.SetImageKey(imageKey);
        var content = Faker.Random.Bytes(256);
        _repository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(product);
        _imageStorage.DownloadAsync(imageKey, Arg.Any<CancellationToken>())
            .Returns(new ProductImageDownload(content, "image/jpeg"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Content.ShouldBe(content);
        result.ContentType.ShouldBe("image/jpeg");
        await _repository.Received(1).GetByIdAsync(query.Id, Arg.Any<CancellationToken>());
        await _imageStorage.Received(1).DownloadAsync(imageKey, Arg.Any<CancellationToken>());
    }

    private static GetProductImageQuery NewQuery() => new(Faker.Random.Int(1, 1000));

    private static Product NewProduct() => new(
        Faker.Commerce.ProductName(),
        Faker.Lorem.Sentence(),
        new Money(Faker.Random.Decimal(1, 1000)),
        Faker.Commerce.Ean13(),
        Faker.Random.Int(1, 1000),
        Faker.Random.Int(0, 500));
}
