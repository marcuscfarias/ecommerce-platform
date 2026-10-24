using Ecommerce.Orders.Api.Carts.ChangeCartItemQuantity;

namespace Ecommerce.Orders.UnitTests.Api.Carts.ChangeCartItemQuantity;

public class ChangeCartItemQuantityRequestValidatorTests
{
    private readonly ChangeCartItemQuantityRequestValidator _sut = new();

    [Theory]
    [InlineData(1)]  // lowest valid quantity
    [InlineData(10)] // highest valid quantity
    public void Validate_WhenQuantityIsWithinOneToTen_ShouldHaveNoErrors(int quantity)
    {
        // Arrange
        var request = new ChangeCartItemQuantityRequest(quantity);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]  // zero means remove, not change
    [InlineData(-1)] // negative quantity
    [InlineData(11)] // one above the maximum
    public void Validate_WhenQuantityIsOutsideOneToTen_ShouldHaveErrorForQuantity(int quantity)
    {
        // Arrange
        var request = new ChangeCartItemQuantityRequest(quantity);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Errors.ShouldContain(e => e.PropertyName == "Quantity");
    }
}
