using Ecommerce.Catalog.Application.Products.UploadProductImage;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Products.UploadProductImage;

public class UploadProductImageHandlerTests
{
    private static readonly Faker Faker = new();
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly IProductImageStorage _imageStorage = Substitute.For<IProductImageStorage>();
    private readonly UploadProductImageHandler _handler;

    public UploadProductImageHandlerTests()
    {
        _handler = new UploadProductImageHandler(_repository, _imageStorage);
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldThrowResourceNotFound()
    {
        // Arrange
        var command = NewCommand();
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductHasNoImage_ShouldUploadAndStoreKey()
    {
        // Arrange
        var command = NewCommand();
        var product = NewProduct();
        var uploadedKey = $"{Faker.Random.Guid():N}.jpg";
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(product);
        _imageStorage.UploadAsync(command.Content, command.ContentType, Arg.Any<CancellationToken>()).Returns(uploadedKey);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.ImageKey.ShouldBe(uploadedKey);
        await _imageStorage.Received(1).UploadAsync(command.Content, command.ContentType, Arg.Any<CancellationToken>());
        _repository.Received(1).Update(product);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductAlreadyHasImage_ShouldDeleteOldBlobBeforeStoringNewKey()
    {
        // Arrange
        var command = NewCommand();
        var existingKey = $"{Faker.Random.Guid():N}.jpg";
        var product = NewProduct();
        product.SetImageKey(existingKey);
        var uploadedKey = $"{Faker.Random.Guid():N}.jpg";
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(product);
        _imageStorage.UploadAsync(command.Content, command.ContentType, Arg.Any<CancellationToken>()).Returns(uploadedKey);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.ImageKey.ShouldBe(uploadedKey);
        await _imageStorage.Received(1).DeleteAsync(existingKey, Arg.Any<CancellationToken>());
        await _imageStorage.Received(1).UploadAsync(command.Content, command.ContentType, Arg.Any<CancellationToken>());
        _repository.Received(1).Update(product);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static UploadProductImageCommand NewCommand() =>
        new(Faker.Random.Int(1, 1000), new MemoryStream([1, 2, 3]), "image/jpeg");

    private static Product NewProduct() => new(
        Faker.Commerce.ProductName(),
        Faker.Lorem.Sentence(),
        new Money(Faker.Random.Decimal(1, 1000)),
        Faker.Commerce.Ean13(),
        Faker.Random.Int(1, 1000),
        Faker.Random.Int(0, 500));
}
