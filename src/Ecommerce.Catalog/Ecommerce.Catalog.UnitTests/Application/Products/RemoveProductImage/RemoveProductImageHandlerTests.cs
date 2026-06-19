using Ecommerce.Catalog.Application.Products.RemoveProductImage;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Products.RemoveProductImage;

public class RemoveProductImageHandlerTests
{
    private static readonly Faker Faker = new();
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly IProductImageStorage _imageStorage = Substitute.For<IProductImageStorage>();
    private readonly RemoveProductImageHandler _handler;

    public RemoveProductImageHandlerTests()
    {
        _handler = new RemoveProductImageHandler(_repository, _imageStorage);
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
    public async Task Handle_WhenProductHasImage_ShouldDeleteBlobAndClearUrl()
    {
        // Arrange
        var command = NewCommand();
        var existingUrl = Faker.Internet.Url();
        var product = NewProduct();
        product.SetImageUrl(existingUrl);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.ImageUrl.ShouldBeNull();
        await _imageStorage.Received(1).DeleteAsync(existingUrl, Arg.Any<CancellationToken>());
        _repository.Received(1).Update(product);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductHasNoImage_ShouldReturnWithoutDeletingOrSaving()
    {
        // Arrange
        var command = NewCommand();
        var product = NewProduct();
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.ImageUrl.ShouldBeNull();
    }

    private static RemoveProductImageCommand NewCommand() => new(Faker.Random.Int(1, 1000));

    private static Product NewProduct() => new(
        Faker.Commerce.ProductName(),
        Faker.Lorem.Sentence(),
        new Money(Faker.Random.Decimal(1, 1000)),
        Faker.Commerce.Ean13(),
        Faker.Random.Int(1, 1000),
        Faker.Random.Int(0, 500));
}
