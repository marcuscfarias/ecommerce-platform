using Ecommerce.Catalog.Application.Categories.Rules;

namespace Ecommerce.Catalog.UnitTests.Application.Categories.Rules;

public class CategorySlugMustBeUniqueRuleTests
{
    [Fact]
    public void IsMet_WhenSlugDoesNotExist_ShouldReturnTrue()
    {
        // Arrange
        var rule = new CategorySlugMustBeUniqueRule(exists: false);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_WhenSlugAlreadyExists_ShouldReturnFalse()
    {
        // Arrange
        var rule = new CategorySlugMustBeUniqueRule(exists: true);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ErrorMessage_ShouldReturnExpectedMessage()
    {
        // Arrange
        var rule = new CategorySlugMustBeUniqueRule(exists: false);

        // Act
        var message = rule.ErrorMessage;

        // Assert
        message.ShouldBe("A category with this slug already exists.");
    }
}
