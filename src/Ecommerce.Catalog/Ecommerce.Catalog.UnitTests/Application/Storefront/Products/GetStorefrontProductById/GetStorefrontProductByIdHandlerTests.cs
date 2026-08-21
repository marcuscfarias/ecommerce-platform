using Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductById;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Storefront.Products.GetStorefrontProductById;

public class GetStorefrontProductByIdHandlerTests
{
    private static readonly Faker Faker = new();

    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly GetStorefrontProductByIdHandler _handler;

    public GetStorefrontProductByIdHandlerTests()
    {
        _handler = new GetStorefrontProductByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenProductIsActive_ShouldReturnTheProjection()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 5);
        var query = new GetStorefrontProductByIdQuery(product.Id);
        _repository.GetActiveByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var expected = new GetStorefrontProductByIdResult(
            product.Id,
            product.Name,
            product.Description,
            product.Price.Amount,
            product.CategoryId,
            InStock: true,
            HasImage: false);
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsNull_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        Product? product = null;
        var query = new GetStorefrontProductByIdQuery(Faker.Random.Int(1, 1000));
        _repository.GetActiveByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductHasStockAndImage_ShouldDeriveBothFlagsAsTrue()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 5);
        product.SetImageKey(Faker.Random.Guid().ToString());
        var query = new GetStorefrontProductByIdQuery(product.Id);
        _repository.GetActiveByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.InStock.ShouldBeTrue();
        result.HasImage.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenProductHasNoStockAndNoImage_ShouldDeriveBothFlagsAsFalse()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 0);
        var query = new GetStorefrontProductByIdQuery(product.Id);
        _repository.GetActiveByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.InStock.ShouldBeFalse();
        result.HasImage.ShouldBeFalse();
    }

    private static Product NewProduct(int stockQuantity) =>
        new(Faker.Commerce.ProductName(),
            Faker.Lorem.Sentence(),
            new Money(Faker.Random.Decimal(1, 1000)),
            Faker.Commerce.Ean13(),
            Faker.Random.Int(1, 100),
            stockQuantity);
}
