using Ecommerce.Orders.Application.Carts.Rules;

namespace Ecommerce.Orders.UnitTests.Application.Carts.Rules;

public class ProductMustBeAvailableRuleTests
{
    [Fact]
    public void IsMet_WhenProductIsActive_ShouldReturnTrue()
    {
        // Arrange
        var rule = new ProductMustBeAvailableRule(isActive: true);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_WhenProductIsInactive_ShouldReturnFalse()
    {
        // Arrange
        var rule = new ProductMustBeAvailableRule(isActive: false);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ErrorMessage_WhenRead_ShouldStateTheProductIsNoLongerAvailable()
    {
        // Arrange
        var rule = new ProductMustBeAvailableRule(isActive: false);

        // Act
        var message = rule.ErrorMessage;

        // Assert
        message.ShouldBe("Product is no longer available.");
    }
}
