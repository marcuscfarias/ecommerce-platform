using Ecommerce.Catalog.Domain.Rules;

namespace Ecommerce.Catalog.UnitTests.Domain.Rules;

public class StockQuantityCannotBeNegativeRuleTests
{
    [Theory]
    [InlineData(-1)] // just below the boundary
    [InlineData(-100)] // far negative
    public void IsMet_WhenStockIsNegative_ShouldReturnFalse(int stockQuantity)
    {
        // Arrange
        var rule = new StockQuantityCannotBeNegativeRule(stockQuantity);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)] // boundary
    [InlineData(50)] // positive
    public void IsMet_WhenStockIsNonNegative_ShouldReturnTrue(int stockQuantity)
    {
        // Arrange
        var rule = new StockQuantityCannotBeNegativeRule(stockQuantity);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeTrue();
    }
}
