using Ecommerce.Auth.Domain.Entities;

namespace Ecommerce.Auth.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        // Arrange
        var email = "user@example.com";
        var passwordHash = "hashed-password";
        var firstName = "John";
        var lastName = "Doe";
        var isActive = true;

        // Act
        var user = new User(email, passwordHash, firstName, lastName, isActive);

        // Assert
        user.Email.ShouldBe(email);
        user.PasswordHash.ShouldBe(passwordHash);
        user.FirstName.ShouldBe(firstName);
        user.LastName.ShouldBe(lastName);
        user.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithIsActiveFalse_ShouldSetIsActiveFalse()
    {
        // Arrange & Act
        var user = new User("user@example.com", "hashed-password", "John", "Doe", isActive: false);

        // Assert
        user.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WithoutIsActive_ShouldDefaultToTrue()
    {
        // Arrange
        var email = "user@example.com";
        var passwordHash = "hashed-password";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = new User(email, passwordHash, firstName, lastName);

        // Assert
        user.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ShouldGenerateNonEmptySecurityStamp()
    {
        // Arrange & Act
        var user = new User("user@example.com", "hash", "John", "Doe");

        // Assert
        user.SecurityStamp.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueSecurityStampPerInstance()
    {
        // Arrange & Act
        var first = new User("a@example.com", "hash", "John", "Doe");
        var second = new User("b@example.com", "hash", "Jane", "Doe");

        // Assert
        first.SecurityStamp.ShouldNotBe(second.SecurityStamp);
    }

}
