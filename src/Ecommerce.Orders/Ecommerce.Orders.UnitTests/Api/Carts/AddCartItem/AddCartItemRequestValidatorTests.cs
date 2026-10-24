using Ecommerce.Orders.Api.Carts.AddCartItem;

namespace Ecommerce.Orders.UnitTests.Api.Carts.AddCartItem;

public class AddCartItemRequestValidatorTests
{
    private readonly AddCartItemRequestValidator _sut = new();

    private static AddCartItemRequest ValidRequest(int productId = 7, int quantity = 1) =>
        new(productId, quantity);

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldHaveNoErrors()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]  // just below the minimum id
    [InlineData(-1)] // negative id
    public void Validate_WhenProductIdIsBelowOne_ShouldHaveErrorForProductId(int productId)
    {
        // Arrange
        var request = ValidRequest(productId: productId);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "ProductId");
    }

    [Theory]
    [InlineData(1)]  // lowest valid quantity
    [InlineData(10)] // highest valid quantity
    public void Validate_WhenQuantityIsWithinOneToTen_ShouldHaveNoErrorForQuantity(int quantity)
    {
        // Arrange
        var request = ValidRequest(quantity: quantity);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldNotContain(e => e.PropertyName == "Quantity");
    }

    [Theory]
    [InlineData(0)]  // zero means remove, not add
    [InlineData(-1)] // negative quantity
    [InlineData(11)] // one above the maximum
    public void Validate_WhenQuantityIsOutsideOneToTen_ShouldHaveErrorForQuantity(int quantity)
    {
        // Arrange
        var request = ValidRequest(quantity: quantity);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Quantity");
    }
}
