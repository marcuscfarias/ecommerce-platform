using Ecommerce.Common.Application.ApplicationRules;

namespace Ecommerce.Common.UnitTests.Application.ApplicationRules;

public class NotFoundApplicationRuleTests
{
    private const string ResourceName = "Category";
    private const string ErrorMessage = $"{ResourceName} was not found.";

    [Fact]
    public void IsMet_EntityIsNotNull_ReturnsTrue()
    {
        //arrange
        var rule = new NotFoundApplicationRule(new object(), ResourceName);

        //act
        var result = rule.IsMet();

        //assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_EntityIsNull_ReturnsFalse()
    {
        //arrange
        var rule = new NotFoundApplicationRule(null, ResourceName);

        //act
        var result = rule.IsMet();

        //assert
        result.ShouldBeFalse();
        rule.Error.ShouldBe(ErrorMessage);
    }

    [Fact]
    public void CreateException_ReturnsApplicationRulesValidationException()
    {
        //arrange
        var rule = new NotFoundApplicationRule(null, ResourceName);

        //act
        var exception = rule.CreateException(rule.Error);

        //assert
        exception.ShouldBeOfType<ApplicationRulesValidationException>();
        exception.Message.ShouldBe(ErrorMessage);
    }
}
