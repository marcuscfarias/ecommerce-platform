using Ecommerce.Common.Domain.BusinessRules.Rules;

namespace Ecommerce.Common.UnitTests.Domain.BusinessRules.Rules;

public class StringNotEmptyRuleTests
{
    private const string FieldName = "FieldName";

    [Fact]
    public void IsMet_ValueHasContent_ReturnsTrue()
    {
        //arrange
        var businessRule = new StringNotEmptyRule("valid", FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_ValueIsNull_ReturnsFalse()
    {
        //arrange
        var businessRule = new StringNotEmptyRule(null, FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMet_ValueIsEmpty_ReturnsFalse()
    {
        //arrange
        var businessRule = new StringNotEmptyRule("", FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMet_ValueIsWhitespace_ReturnsFalse()
    {
        //arrange
        var businessRule = new StringNotEmptyRule(" ", FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CreateException_ReturnsBusinessRulesValidationException()
    {
        //arrange
        var businessRule = new StringNotEmptyRule(null, FieldName);

        //act
        var exception = businessRule.CreateException();

        //assert
        exception.ShouldBeOfType<ArgumentNullException>();
    }
}