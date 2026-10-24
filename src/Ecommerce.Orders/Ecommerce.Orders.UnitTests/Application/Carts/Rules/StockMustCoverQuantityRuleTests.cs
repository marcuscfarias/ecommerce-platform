using Ecommerce.Orders.Application.Carts.Rules;

namespace Ecommerce.Orders.UnitTests.Application.Carts.Rules;

public class StockMustCoverQuantityRuleTests
{
    [Theory]
    [InlineData(3, 3)] // quantity equal to stock
    [InlineData(5, 2)] // quantity below stock
    public void IsMet_WhenStockCoversQuantity_ShouldReturnTrue(int stockQuantity, int quantity)
    {
        // Arrange
        var rule = new StockMustCoverQuantityRule(stockQuantity, quantity);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(3, 4)] // one unit above stock
    [InlineData(1, 10)] // far above stock
    public void IsMet_WhenQuantityExceedsStock_ShouldReturnFalse(int stockQuantity, int quantity)
    {
        // Arrange
        var rule = new StockMustCoverQuantityRule(stockQuantity, quantity);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ErrorMessage_WhenRead_ShouldStateTheAvailableStock()
    {
        // Arrange
        var rule = new StockMustCoverQuantityRule(2, 5);

        // Act
        var message = rule.ErrorMessage;

        // Assert
        message.ShouldBe("Only 2 available.");
    }
}
