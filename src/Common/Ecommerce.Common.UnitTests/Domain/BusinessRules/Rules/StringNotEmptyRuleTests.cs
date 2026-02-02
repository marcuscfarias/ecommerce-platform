using Ecommerce.Common.Domain.BusinessRules.Rules;

namespace Ecommerce.Common.UnitTests.Domain.BusinessRules.Rules;

public class StringNotEmptyRuleTests
{
    private const string FieldName = "FieldName";
    private const string ErrorMessage = $"{FieldName} cannot be empty.";

    [Fact]
    public void IsMet_ValueHasContent_ReturnsTrue()
    {
        //arrange
        var businessRule = new StringNotEmptyBusinessRule("valid", FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_ValueIsNull_ReturnsFalseWithErrorMessage()
    {
        //arrange
        var businessRule = new StringNotEmptyBusinessRule(null, FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeFalse();
        businessRule.Error.ShouldBe(ErrorMessage);
    }

    [Fact]
    public void IsMet_ValueIsEmpty_ReturnsFalseWithErrorMessage()
    {
        //arrange
        var businessRule = new StringNotEmptyBusinessRule("", FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeFalse();
        businessRule.Error.ShouldBe(ErrorMessage);
    }

    [Fact]
    public void IsMet_ValueIsWhitespace_ReturnsFalseWithErrorMessage()
    {
        //arrange
        var businessRule = new StringNotEmptyBusinessRule(" ", FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeFalse();
        businessRule.Error.ShouldBe(ErrorMessage);
    }

    [Fact]
    public void CreateException_ReturnsBusinessRulesValidationException()
    {
        //arrange
        var businessRule = new StringNotEmptyBusinessRule(null, FieldName);

        //act
        var exception = businessRule.CreateException(businessRule.Error);

        //assert
        exception.ShouldBeOfType<BusinessRulesValidationException>();
        exception.Message.ShouldBe(ErrorMessage);
    }
}