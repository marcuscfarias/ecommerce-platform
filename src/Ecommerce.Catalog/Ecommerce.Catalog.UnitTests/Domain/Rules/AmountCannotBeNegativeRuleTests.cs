using Ecommerce.Catalog.Domain.Rules;

namespace Ecommerce.Catalog.UnitTests.Domain.Rules;

public class AmountCannotBeNegativeRuleTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void IsMet_WhenAmountIsNegative_ShouldReturnFalse()
    {
        // Arrange
        var rule = new AmountCannotBeNegativeRule(-0.01m);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMet_WhenAmountIsZero_ShouldReturnTrue()
    {
        // Arrange
        var rule = new AmountCannotBeNegativeRule(0m);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_WhenAmountIsPositive_ShouldReturnTrue()
    {
        // Arrange
        var amount = Faker.Random.Decimal(0.01m, 1000);
        var rule = new AmountCannotBeNegativeRule(amount);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeTrue();
    }
}
