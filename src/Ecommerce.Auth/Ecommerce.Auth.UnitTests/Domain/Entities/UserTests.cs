using Ecommerce.Auth.Domain.Entities;

namespace Ecommerce.Auth.UnitTests.Domain.Entities;

public class UserTests
{
    private readonly Faker _faker = new();


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

    [Fact]
    public void UpdateProfile_WhenCalled_ShouldUpdateFirstNameLastNameAndIsActive()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), "hash", _faker.Name.FirstName(), _faker.Name.LastName());
        var newFirstName = _faker.Name.FirstName();
        var newLastName = _faker.Name.LastName();

        // Act
        user.UpdateProfile(newFirstName, newLastName, isActive: false);

        // Assert
        user.FirstName.ShouldBe(newFirstName);
        user.LastName.ShouldBe(newLastName);
        user.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void UpdateProfile_WhenCalled_ShouldNotChangeEmailOrPasswordHash()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var passwordHash = _faker.Internet.Password();
        var user = new User(email, passwordHash, _faker.Name.FirstName(), _faker.Name.LastName());

        // Act
        user.UpdateProfile(_faker.Name.FirstName(), _faker.Name.LastName(), isActive: true);

        // Assert
        user.Email.ShouldBe(email);
        user.PasswordHash.ShouldBe(passwordHash);
    }

}
