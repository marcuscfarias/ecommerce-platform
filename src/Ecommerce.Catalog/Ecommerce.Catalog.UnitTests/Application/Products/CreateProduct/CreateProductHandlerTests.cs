using Ecommerce.Catalog.Application.Exceptions;
using Ecommerce.Catalog.Application.Products.CreateProduct;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Products.CreateProduct;

public class CreateProductHandlerTests
{
    private static readonly Faker Faker = new();
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _handler = new CreateProductHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldAddProductAndSaveChanges()
    {
        // Arrange
        var command = ValidCommand();
        _repository.CheckCategoryExistsAsync(command.CategoryId, Arg.Any<CancellationToken>()).Returns(true);
        _repository.CheckSkuExistsAsync(command.Sku, Arg.Any<int?>(), Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Add(Arg.Any<Product>());
        await _repository.Received(1).CheckCategoryExistsAsync(command.CategoryId, Arg.Any<CancellationToken>());
        await _repository.Received(1).CheckSkuExistsAsync(command.Sku, Arg.Any<int?>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ShouldThrowResourceNotFound()
    {
        // Arrange
        var command = ValidCommand();
        _repository.CheckCategoryExistsAsync(command.CategoryId, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSkuAlreadyExists_ShouldThrowSkuConflict()
    {
        // Arrange
        var command = ValidCommand();
        _repository.CheckCategoryExistsAsync(command.CategoryId, Arg.Any<CancellationToken>()).Returns(true);
        _repository.CheckSkuExistsAsync(command.Sku, Arg.Any<int?>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<SkuConflictException>();
    }

    private static CreateProductCommand ValidCommand() => new(
        Faker.Commerce.ProductName(),
        Faker.Lorem.Sentence(),
        Faker.Random.Decimal(1, 1000),
        Faker.Commerce.Ean13(),
        Faker.Random.Int(1, 1000),
        Faker.Random.Int(0, 500));
}
