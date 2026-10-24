using Ecommerce.Catalog.Application.Products.GetProductAvailability;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Catalog.UnitTests.Application.Products.GetProductAvailability;

public class GetProductAvailabilityHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly Faker _faker = new();
    private readonly GetProductAvailabilityHandler _handler;

    public GetProductAvailabilityHandlerTests()
    {
        _handler = new GetProductAvailabilityHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenProductsExist_ShouldProjectCurrentNamePriceStockAndImageFlag()
    {
        // Arrange
        var withImage = NewProduct(id: 1);
        withImage.SetImageKey($"{_faker.Random.Guid():N}.jpg");
        var withoutImage = NewProduct(id: 2);
        var query = new GetProductAvailabilityQuery([withImage.Id, withoutImage.Id]);
        ArrangeRepository(query, withImage, withoutImage);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.ShouldBe([
            new ProductAvailability(withImage.Id, withImage.Name, withImage.Price.Amount, withImage.StockQuantity, IsActive: true, HasImage: true),
            new ProductAvailability(withoutImage.Id, withoutImage.Name, withoutImage.Price.Amount, withoutImage.StockQuantity, IsActive: true, HasImage: false),
        ]);
    }

    [Fact]
    public async Task Handle_WhenProductIsInactive_ShouldReturnItWithIsActiveFalse()
    {
        // Arrange
        var product = NewProduct(id: 1);
        product.Deactivate();
        var query = new GetProductAvailabilityQuery([product.Id]);
        ArrangeRepository(query, product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var item = result.Items.ShouldHaveSingleItem();
        item.Id.ShouldBe(product.Id);
        item.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenSomeProductsAreUnknown_ShouldReturnOnlyTheExistingOnes()
    {
        // Arrange
        var product = NewProduct(id: 1);
        var query = new GetProductAvailabilityQuery([product.Id, _faker.Random.Int(1000, 2000)]);
        ArrangeRepository(query, product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.ShouldHaveSingleItem().Id.ShouldBe(product.Id);
    }

    [Fact]
    public async Task Handle_WhenNoProductExists_ShouldReturnNoItems()
    {
        // Arrange
        var query = new GetProductAvailabilityQuery([_faker.Random.Int(1000, 2000)]);
        ArrangeRepository(query);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.ShouldBeEmpty();
    }

    private void ArrangeRepository(GetProductAvailabilityQuery query, params Product[] products) =>
        _repository.GetByIdsAsync(query.ProductIds, Arg.Any<CancellationToken>())
            .Returns(products);

    private Product NewProduct(int id)
    {
        var product = new Product(
            _faker.Commerce.ProductName(),
            _faker.Lorem.Sentence(),
            new Money(_faker.Random.Decimal(1, 1000)),
            _faker.Commerce.Ean13(),
            _faker.Random.Int(1, 100),
            _faker.Random.Int(0, 500));
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(product, id);
        return product;
    }
}
