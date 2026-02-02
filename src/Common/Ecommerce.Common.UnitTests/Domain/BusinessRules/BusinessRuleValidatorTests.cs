using Ecommerce.Common.Domain.BusinessRules;

namespace Ecommerce.Common.UnitTests.Domain.BusinessRules;

public class BusinessRuleValidatorTests
{
    [Fact]
    public void Validate_RuleIsMet_DoesNotThrow()
    {
        //arrange
        var businessRule = new FakeBusinessRule(11);

        //act & assert
        Should.NotThrow(() => BusinessRuleValidator.Validate(businessRule));
    }

    [Fact]
    public void Validate_RuleIsNotMet_ThrowsExceptionWithErrorMessage()
    {
        //arrange
        var businessRule = new FakeBusinessRule(10);

        //act
        var exception = Should.Throw<Exception>(
            () => BusinessRuleValidator.Validate(businessRule));

        //assert
        exception.Message.ShouldBe(businessRule.Error);
    }
}
