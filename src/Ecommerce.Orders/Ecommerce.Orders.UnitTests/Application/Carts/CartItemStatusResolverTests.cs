using Ecommerce.Catalog.Application.Products.GetProductAvailability;
using Ecommerce.Orders.Application.Carts;

namespace Ecommerce.Orders.UnitTests.Application.Carts;

public class CartItemStatusResolverTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Resolve_WhenProductIsInactive_ShouldReturnUnavailable()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 5, isActive: false);

        // Act
        var status = CartItemStatusResolver.Resolve(product, quantity: 1);

        // Assert
        status.ShouldBe(CartItemStatus.Unavailable);
    }

    [Fact]
    public void Resolve_WhenProductIsMissing_ShouldReturnUnavailable()
    {
        // Act
        var status = CartItemStatusResolver.Resolve(null, quantity: 1);

        // Assert
        status.ShouldBe(CartItemStatus.Unavailable);
    }

    [Fact]
    public void Resolve_WhenProductIsInactiveAndOutOfStock_ShouldReturnUnavailable()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 0, isActive: false);

        // Act
        var status = CartItemStatusResolver.Resolve(product, quantity: 1);

        // Assert
        status.ShouldBe(CartItemStatus.Unavailable);
    }

    [Fact]
    public void Resolve_WhenStockIsZero_ShouldReturnOutOfStock()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 0, isActive: true);

        // Act
        var status = CartItemStatusResolver.Resolve(product, quantity: 3);

        // Assert
        status.ShouldBe(CartItemStatus.OutOfStock);
    }

    [Fact]
    public void Resolve_WhenQuantityExceedsStock_ShouldReturnInsufficientStock()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 2, isActive: true);

        // Act
        var status = CartItemStatusResolver.Resolve(product, quantity: 3);

        // Assert
        status.ShouldBe(CartItemStatus.InsufficientStock);
    }

    [Theory]
    [InlineData(3, 3)] // quantity equal to stock
    [InlineData(10, 3)] // stock above quantity
    public void Resolve_WhenStockCoversQuantity_ShouldReturnAvailable(int stockQuantity, int quantity)
    {
        // Arrange
        var product = NewProduct(stockQuantity, isActive: true);

        // Act
        var status = CartItemStatusResolver.Resolve(product, quantity);

        // Assert
        status.ShouldBe(CartItemStatus.Available);
    }

    private ProductAvailability NewProduct(int stockQuantity, bool isActive) =>
        new(_faker.Random.Int(1),
            _faker.Commerce.ProductName(),
            _faker.Random.Decimal(1, 1000),
            stockQuantity,
            isActive,
            _faker.Random.Bool());
}
