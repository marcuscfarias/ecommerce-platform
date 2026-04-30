using Ecommerce.Kernel.Domain.BusinessRules;
using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Kernel.UnitTests.Domain.BusinessRules;

public class BusinessRuleValidatorTests
{
    [Fact]
    public void Validate_RuleIsMet_ShouldNotThrow()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 20);

        // Act
        var act = () => BusinessRule.Validate(rule);

        // Assert
        act.ShouldNotThrow();
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
    public void Validate_RuleIsNotMet_ShouldThrowFromCreateException()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 5);

        // Act
        var act = () => BusinessRule.Validate(rule);

        // Assert
        act.ShouldThrow<BusinessRuleValidationException>();
        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Validate_RuleIsNotMetAtBoundary_ShouldThrow()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 10);

        // Act
        var act = () => BusinessRule.Validate(rule);

        // Assert
        act.ShouldThrow<BusinessRuleValidationException>();
        act.ShouldThrow<InvalidOperationException>();
    }
}
