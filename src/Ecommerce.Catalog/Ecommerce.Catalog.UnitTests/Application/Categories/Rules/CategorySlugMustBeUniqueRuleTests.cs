using Ecommerce.Catalog.Application.Categories.Rules;
using Ecommerce.Shared.Domain.BusinessRules;
using Ecommerce.Shared.Domain.Exceptions;

namespace Ecommerce.Catalog.UnitTests.Application.Categories.Rules;

public class CategorySlugMustBeUniqueRuleTests
{
    [Fact]
    public void IsMet_WhenSlugDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var rule = new CategorySlugMustBeUniqueRule(exists: false);

        // Act
        var act = () => BusinessRule.Validate(rule);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void IsMet_WhenSlugAlreadyExists_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var rule = new CategorySlugMustBeUniqueRule(exists: true);

        // Act
        var act = () => BusinessRule.Validate(rule);

        // Assert
        act.ShouldThrow<BusinessRuleValidationException>();
    }
}
