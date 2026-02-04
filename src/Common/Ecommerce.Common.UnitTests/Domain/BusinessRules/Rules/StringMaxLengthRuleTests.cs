using Ecommerce.Common.Domain.BusinessRules.Rules;

namespace Ecommerce.Common.UnitTests.Domain.BusinessRules.Rules;

public class StringMaxLengthRuleTests
{
    private const int MaxLength = 5;
    private const string FieldName = "FieldName";

    [Fact]
    public void IsMet_ValueIsNull_ReturnsTrue()
    {
        //arrange
        var businessRule = new StringMaxLengthRule(null, MaxLength, FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeTrue();
    }
    
    [Fact]
    public void IsMet_ValueIsBelowMaxLength_ReturnsTrue()
    {
        //arrange
        var businessRule = new StringMaxLengthRule("xxxx", MaxLength, FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeTrue();
    }
    
    [Fact]
    public void IsMet_ValueIsEqualToMaxLength_ReturnsTrue()
    {
        //arrange
        var businessRule = new StringMaxLengthRule("xxxxx", MaxLength, FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_ValueIsLongerThanMaxValue_ReturnsFalse()
    {
        //arrange
        var businessRule = new StringMaxLengthRule("xxxxxx", MaxLength, FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CreateException_ValueIsLongerThanMaxValue_ReturnsBusinessRulesValidationException()
    {
        //arrange
        var businessRule = new StringMaxLengthRule("xxxxxx", MaxLength, FieldName);

        //act
        var exception = businessRule.CreateException();

        //assert
        exception.ShouldBeOfType<ArgumentOutOfRangeException>();
    }
}