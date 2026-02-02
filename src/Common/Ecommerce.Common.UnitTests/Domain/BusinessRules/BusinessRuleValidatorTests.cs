using Ecommerce.Common.Domain.BusinessRules;

namespace Ecommerce.Common.UnitTests.Domain.BusinessRules;

public class BusinessRuleValidatorTests
{
    [Fact]
    public void Validate_RuleIsMet_DoesNotThrow()
    {
        //arrange
        var rule = new FakeBusinessRule(11);

        //act & assert
        Should.NotThrow(() => BusinessRuleValidator.Validate(rule));
    }

    [Fact]
    public void Validate_RuleIsNotMet_ThrowsBusinessRuleValidationExceptionWithErrorMessage()
    {
        //arrange
        var rule = new FakeBusinessRule(10);

        //act
        var exception = Should.Throw<BusinessRuleValidationException>(
            () => BusinessRuleValidator.Validate(rule));

        //assert
        exception.Message.ShouldBe(rule.Error);
    }
}
