using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Domain.Products;

public class MoneyTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Constructor_WithAmount_ShouldSetAmountAndDefaultCurrency()
    {
        // Arrange
        var amount = Faker.Random.Decimal(0, 1000);

        // Act
        var money = new Money(amount);

        // Assert
        money.Amount.ShouldBe(amount);
        money.Currency.ShouldBe("USD");
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldThrowBusinessRuleValidation()
    {
        // Act
        var act = () => new Money(-1m);

        // Assert
        Should.Throw<BusinessRuleValidationException>(act);
    }

    [Fact]
    public void Equality_WithSameAmount_ShouldBeEqual()
    {
        // Arrange
        var first = new Money(19.90m);
        var second = new Money(19.90m);

        // Assert
        first.ShouldBe(second);
    }
}
