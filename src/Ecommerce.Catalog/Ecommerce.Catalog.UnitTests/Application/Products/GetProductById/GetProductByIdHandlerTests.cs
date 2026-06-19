using Ecommerce.Catalog.Application.Products.GetProductById;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Products.GetProductById;

public class GetProductByIdHandlerTests
{
    private static readonly Faker Faker = new();
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly GetProductByIdHandler _handler;

    public GetProductByIdHandlerTests()
    {
        _handler = new GetProductByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        Product? product = null;
        var query = new GetProductByIdQuery(Faker.Random.Int(1, 1000));
        _repository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductExists_ShouldReturnMappedResult()
    {
        // Arrange
        var product = new Product(
            Faker.Commerce.ProductName(),
            Faker.Lorem.Sentence(),
            new Money(Faker.Random.Decimal(1, 1000)),
            Faker.Commerce.Ean13(),
            Faker.Random.Int(1, 1000),
            Faker.Random.Int(0, 500));
        var query = new GetProductByIdQuery(Faker.Random.Int(1, 1000));
        _repository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var expected = new GetProductByIdResult(
            product.Id,
            product.Name,
            product.Description,
            product.Price.Amount,
            product.Price.Currency,
            product.Sku,
            product.CategoryId,
            product.StockQuantity,
            product.IsActive,
            product.ImageUrl);
        result.ShouldBe(expected);
    }
}
