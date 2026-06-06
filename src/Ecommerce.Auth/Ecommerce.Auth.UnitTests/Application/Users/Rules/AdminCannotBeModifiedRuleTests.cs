using Ecommerce.Auth.Application.Users.Rules;

namespace Ecommerce.Auth.UnitTests.Application.Users.Rules;

public class AdminCannotBeModifiedRuleTests
{
    [Fact]
    public void IsMet_WhenUserIsAdmin_ShouldReturnFalse()
    {
        // Arrange
        var rule = new AdminCannotBeModifiedRule(isAdmin: true);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMet_WhenUserIsNotAdmin_ShouldReturnTrue()
    {
        // Arrange
        var rule = new AdminCannotBeModifiedRule(isAdmin: false);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeTrue();
    }
}
