using Ecommerce.Orders.Application.Carts.Rules;

namespace Ecommerce.Orders.UnitTests.Application.Carts.Rules;

public class ProductMustBeInStockRuleTests
{
    [Theory]
    [InlineData(1)] // just above the boundary
    [InlineData(50)] // well stocked
    public void IsMet_WhenStockIsPositive_ShouldReturnTrue(int stockQuantity)
    {
        // Arrange
        var rule = new ProductMustBeInStockRule(stockQuantity);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_WhenStockIsZero_ShouldReturnFalse()
    {
        // Arrange
        var rule = new ProductMustBeInStockRule(0);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ErrorMessage_WhenRead_ShouldStateTheProductIsOutOfStock()
    {
        // Arrange
        var rule = new ProductMustBeInStockRule(0);

        // Act
        var message = rule.ErrorMessage;

        // Assert
        message.ShouldBe("Product is out of stock.");
    }
}
