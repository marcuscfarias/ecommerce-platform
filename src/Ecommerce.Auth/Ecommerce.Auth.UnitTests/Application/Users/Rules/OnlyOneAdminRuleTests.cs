using Ecommerce.Auth.Application.Users.Rules;

namespace Ecommerce.Auth.UnitTests.Application.Users.Rules;

public class OnlyOneAdminRuleTests
{
    [Fact]
    public void IsMet_WhenAssigningAdminRole_ShouldReturnFalse()
    {
        // Arrange
        var rule = new OnlyOneAdminRule(roleIsAdmin: true);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMet_WhenNotAssigningAdminRole_ShouldReturnTrue()
    {
        // Arrange
        var rule = new OnlyOneAdminRule(roleIsAdmin: false);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeTrue();
    }
}
