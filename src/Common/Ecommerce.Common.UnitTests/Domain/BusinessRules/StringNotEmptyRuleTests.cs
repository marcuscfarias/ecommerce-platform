using Ecommerce.Common.Domain.BusinessRules.Rules;
using Shouldly;

namespace Ecommerce.Common.UnitTests.Domain.BusinessRules;

public class StringNotEmptyRuleTests
{
    private const string FieldName = "FieldName";
    private const string ErrorMessage = $"{FieldName} cannot be empty.";

    [Fact]
    public void IsMet_ValueHasContent_ReturnsTrue()
    {
        //arrange
        var rule = new StringNotEmptyRule("valid", FieldName);

        //act
        var result = rule.IsMet();

        //assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_ValueIsNull_ReturnsFalseWithErrorMessage()
    {
        //arrange
        var rule = new StringNotEmptyRule(null, FieldName);

        //act
        var result = rule.IsMet();

        //assert
        result.ShouldBeFalse();
        rule.Error.ShouldBe(ErrorMessage);
    }

    [Fact]
    public void IsMet_ValueIsEmpty_ReturnsFalseWithErrorMessage()
    {
        //arrange
        var rule = new StringNotEmptyRule("", FieldName);

        //act
        var result = rule.IsMet();

        //assert
        result.ShouldBeFalse();
        rule.Error.ShouldBe(ErrorMessage);
    }

    [Fact]
    public void IsMet_ValueIsWhitespace_ReturnsFalseWithErrorMessage()
    {
        //arrange
        var rule = new StringNotEmptyRule(" ", FieldName);

        //act
        var result = rule.IsMet();

        //assert
        result.ShouldBeFalse();
        rule.Error.ShouldBe(ErrorMessage);
    }
}