using Ecommerce.Catalog.Application;
using Ecommerce.Catalog.Application.Products.GetProductAvailability;
using Ecommerce.Orders.Application.Carts;

namespace Ecommerce.Orders.UnitTests.Application.Carts;

public class CatalogProductsTests
{
    private readonly ICatalogModule _catalogModule = Substitute.For<ICatalogModule>();
    private readonly Faker _faker = new();
    private readonly CatalogProducts _catalogProducts;

    public CatalogProductsTests()
    {
        _catalogProducts = new CatalogProducts(_catalogModule);
    }

    [Fact]
    public async Task GetAsync_WhenIdsAreGiven_ShouldQueryCatalogOnceWithAllIds()
    {
        // Arrange
        int[] productIds = [_faker.Random.Int(1, 1000), _faker.Random.Int(1001, 2000)];
        _catalogModule.ExecuteQueryAsync(Arg.Any<GetProductAvailabilityQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetProductAvailabilityResult([]));

        // Act
        await _catalogProducts.GetAsync(productIds, CancellationToken.None);

        // Assert
        await _catalogModule.Received(1).ExecuteQueryAsync(
            Arg.Is<GetProductAvailabilityQuery>(q => q.ProductIds.SequenceEqual(productIds)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_WhenCatalogReturnsProducts_ShouldKeyThemByProductId()
    {
        // Arrange
        var first = NewProduct(_faker.Random.Int(1, 1000));
        var second = NewProduct(_faker.Random.Int(1001, 2000));
        _catalogModule.ExecuteQueryAsync(Arg.Any<GetProductAvailabilityQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetProductAvailabilityResult([first, second]));

        // Act
        var result = await _catalogProducts.GetAsync([first.Id, second.Id], CancellationToken.None);

        // Assert
        result.Count.ShouldBe(2);
        result[first.Id].ShouldBe(first);
        result[second.Id].ShouldBe(second);
    }

    [Fact]
    public async Task GetAsync_WhenIdsAreEmpty_ShouldReturnEmptyWithoutQueryingCatalog()
    {
        // Act
        var result = await _catalogProducts.GetAsync([], CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    private ProductAvailability NewProduct(int id) =>
        new(id,
            _faker.Commerce.ProductName(),
            _faker.Random.Decimal(1, 1000),
            _faker.Random.Int(0, 50),
            _faker.Random.Bool(),
            _faker.Random.Bool());
}
