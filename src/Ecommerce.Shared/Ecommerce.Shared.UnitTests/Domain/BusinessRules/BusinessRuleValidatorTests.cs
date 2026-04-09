using Ecommerce.Shared.Domain.BusinessRules;

namespace Ecommerce.Shared.UnitTests.Domain.BusinessRules;

public class BusinessRuleValidatorTests
{
    [Fact]
    public void Validate_RuleIsMet_ShouldNotThrow()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 11);

        // Act
        var act = () => BusinessRule.Validate(rule);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void Validate_RuleIsNotMet_ShouldThrowFromCreateException()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 5);

        // Act
        var act = () => BusinessRule.Validate(rule);

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Validate_RuleIsNotMet_ExceptionMessageShouldMatchRuleError()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 5);

        // Act
        var exception = Should.Throw<InvalidOperationException>(
            () => BusinessRule.Validate(rule));

        // Assert
        exception.Message.ShouldBe("Fake business rule was not met");
    }

    [Fact]
    public void Validate_RuleIsMetAtBoundary_ShouldNotThrow()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 11);

        // Act
        var act = () => BusinessRule.Validate(rule);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void Validate_RuleIsNotMetAtBoundary_ShouldThrow()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 10);

        // Act
        var act = () => BusinessRule.Validate(rule);

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }
}
