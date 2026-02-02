using Ecommerce.Common.Domain.BusinessRules.Rules;
using Shouldly;

namespace Ecommerce.Common.UnitTests.Domain.BusinessRules;

public class StringMaxLengthRuleTests
{
    private const int MaxLength = 5;
    private const string FieldName = "FieldName";

    [Fact]
    public void IsMet_ValueIsNull_ReturnsTrue()
    {
        //arrange
        var rule = new StringMaxLengthRule(null, MaxLength, FieldName);

        //act
        var result = rule.IsMet();

        //assert
        result.ShouldBeTrue();
    }
    
    [Fact]
    public void IsMet_ValueIsBelowMaxLength_ReturnsTrue()
    {
        //arrange
        var rule = new StringMaxLengthRule("xxxx", MaxLength, FieldName);

        //act
        var result = rule.IsMet();

        //assert
        result.ShouldBeTrue();
    }
    
    [Fact]
    public void IsMet_ValueIsEqualToMaxLength_ReturnsTrue()
    {
        //arrange
        var rule = new StringMaxLengthRule("xxxxx", MaxLength, FieldName);

        //act
        var result = rule.IsMet();

        //assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_ValueIsLongerThanMaxValue_ReturnsFalseWithErrorMessage()
    {
        //arrange
        var rule = new StringMaxLengthRule("xxxxxx", MaxLength, FieldName);

        //act
        var result = rule.IsMet();

        //assert
        result.ShouldBeFalse();
        rule.Error.ShouldBe($"{FieldName} must be at most {MaxLength} characters.");
    }
}