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
        var businessRule = new StringMaxLengthBusinessRule(null, MaxLength, FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeTrue();
    }
    
    [Fact]
    public void IsMet_ValueIsBelowMaxLength_ReturnsTrue()
    {
        //arrange
        var businessRule = new StringMaxLengthBusinessRule("xxxx", MaxLength, FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeTrue();
    }
    
    [Fact]
    public void IsMet_ValueIsEqualToMaxLength_ReturnsTrue()
    {
        //arrange
        var businessRule = new StringMaxLengthBusinessRule("xxxxx", MaxLength, FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_ValueIsLongerThanMaxValue_ReturnsFalseWithErrorMessage()
    {
        //arrange
        var businessRule = new StringMaxLengthBusinessRule("xxxxxx", MaxLength, FieldName);

        //act
        var result = businessRule.IsMet();

        //assert
        result.ShouldBeFalse();
        businessRule.Error.ShouldBe($"{FieldName} must be at most {MaxLength} characters.");
    }

    [Fact]
    public void CreateException_ReturnsBusinessRulesValidationException()
    {
        //arrange
        var businessRule = new StringMaxLengthBusinessRule("xxxxxx", MaxLength, FieldName);

        //act
        var exception = businessRule.CreateException(businessRule.Error);

        //assert
        exception.ShouldBeOfType<BusinessRulesValidationException>();
        exception.Message.ShouldBe($"{FieldName} must be at most {MaxLength} characters.");
    }
}