using Ecommerce.Catalog.Application.Exceptions;
using Ecommerce.Catalog.Application.Products.UpdateProduct;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Products.UpdateProduct;

public class UpdateProductHandlerTests
{
    private static readonly Faker Faker = new();
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _handler = new UpdateProductHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ShouldThrowResourceNotFound()
    {
        // Arrange
        var command = ValidCommand();
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ShouldThrowResourceNotFound()
    {
        // Arrange
        var command = ValidCommand();
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(ExistingProduct());
        _repository.CheckCategoryExistsAsync(command.CategoryId, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSkuAlreadyExists_ShouldThrowSkuConflict()
    {
        // Arrange
        var command = ValidCommand();
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(ExistingProduct());
        _repository.CheckCategoryExistsAsync(command.CategoryId, Arg.Any<CancellationToken>()).Returns(true);
        _repository.CheckSkuExistsAsync(command.Sku, command.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<SkuConflictException>();
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldUpdateProductAndSaveChanges()
    {
        // Arrange
        var command = ValidCommand();
        var product = ExistingProduct();
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.CheckCategoryExistsAsync(command.CategoryId, Arg.Any<CancellationToken>()).Returns(true);
        _repository.CheckSkuExistsAsync(command.Sku, command.Id, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Name.ShouldBe(command.Name);
        product.Description.ShouldBe(command.Description);
        product.Price.Amount.ShouldBe(command.Price);
        product.Sku.ShouldBe(command.Sku);
        product.CategoryId.ShouldBe(command.CategoryId);
        product.StockQuantity.ShouldBe(command.StockQuantity);
        _repository.Received(1).Update(product);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static UpdateProductCommand ValidCommand() => new(
        Faker.Random.Int(1, 1000),
        Faker.Commerce.ProductName(),
        Faker.Lorem.Sentence(),
        Faker.Random.Decimal(1, 1000),
        Faker.Commerce.Ean13(),
        Faker.Random.Int(1, 1000),
        Faker.Random.Int(0, 500));

    private static Product ExistingProduct() => new(
        Faker.Commerce.ProductName(),
        Faker.Lorem.Sentence(),
        new Money(Faker.Random.Decimal(1, 1000)),
        Faker.Commerce.Ean13(),
        Faker.Random.Int(1, 1000),
        Faker.Random.Int(0, 500));
}
