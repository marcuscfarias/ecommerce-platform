using Ecommerce.Auth.Application.Users.Rules;

namespace Ecommerce.Auth.UnitTests.Application.Users.Rules;

public class EmailMustBeUniqueRuleTests
{
    [Fact]
    public void IsMet_WhenEmailDoesNotExist_ShouldReturnTrue()
    {
        // Arrange
        var rule = new EmailMustBeUniqueRule(exists: false);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsMet_WhenEmailAlreadyExists_ShouldReturnFalse()
    {
        // Arrange
        var rule = new EmailMustBeUniqueRule(exists: true);

        // Act
        var result = rule.IsMet();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ErrorMessage_ShouldReturnExpectedMessage()
    {
        // Arrange
        var rule = new EmailMustBeUniqueRule(exists: false);

        // Act
        var message = rule.ErrorMessage;

        // Assert
        message.ShouldBe("A user with this email already exists.");
    }
}
