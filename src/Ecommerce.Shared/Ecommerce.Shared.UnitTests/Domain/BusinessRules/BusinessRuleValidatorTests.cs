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
        var act = () => BusinessRuleValidator.Validate(rule);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void Validate_RuleIsNotMet_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 5);

        // Act
        var act = () => BusinessRuleValidator.Validate(rule);

        // Assert
        act.ShouldThrow<BusinessRuleValidationException>();
    }

    [Fact]
    public void Validate_RuleIsNotMet_ExceptionMessageShouldMatchRuleError()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 5);

        // Act
        var exception = Should.Throw<BusinessRuleValidationException>(
            () => BusinessRuleValidator.Validate(rule));

        // Assert
        exception.Message.ShouldBe(rule.Error);
    }

    [Fact]
    public void Validate_RuleIsNotMet_ExceptionShouldBeInvalidOperationException()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 0);

        // Act
        var exception = Should.Throw<BusinessRuleValidationException>(
            () => BusinessRuleValidator.Validate(rule));

        // Assert
        exception.ShouldBeAssignableTo<InvalidOperationException>();
    }

    [Fact]
    public void Validate_RuleIsMetAtBoundary_ShouldNotThrow()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 11);

        // Act
        var act = () => BusinessRuleValidator.Validate(rule);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void Validate_RuleIsNotMetAtBoundary_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var rule = new FakeBusinessRule(someNumber: 10);

        // Act
        var act = () => BusinessRuleValidator.Validate(rule);

        // Assert
        act.ShouldThrow<BusinessRuleValidationException>();
    }
}
