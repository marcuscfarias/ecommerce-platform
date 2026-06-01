using Ecommerce.Auth.Domain.Entities;

namespace Ecommerce.Auth.UnitTests.Domain.Entities;

public class UserTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var passwordHash = _faker.Random.AlphaNumeric(60);
        var name = _faker.Name.FullName();
        var isActive = true;

        // Act
        var user = new User(email, passwordHash, name, isActive);

        // Assert
        user.Email.ShouldBe(email);
        user.PasswordHash.ShouldBe(passwordHash);
        user.Name.ShouldBe(name);
        user.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_WithIsActiveFalse_ShouldSetIsActiveFalse()
    {
        // Arrange & Act
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName(), isActive: false);

        // Assert
        user.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WithoutIsActive_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());

        // Assert
        user.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void UpdateProfile_WhenCalled_ShouldUpdateName()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var newName = _faker.Name.FullName();

        // Act
        user.UpdateProfile(newName);

        // Assert
        user.Name.ShouldBe(newName);
    }

    [Fact]
    public void UpdateProfile_WhenCalled_ShouldNotChangeEmailOrPasswordHash()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var passwordHash = _faker.Random.AlphaNumeric(60);
        var user = new User(email, passwordHash, _faker.Name.FullName());

        // Act
        user.UpdateProfile(_faker.Name.FullName());

        // Assert
        user.Email.ShouldBe(email);
        user.PasswordHash.ShouldBe(passwordHash);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WhenCalled_ShouldGenerateSecurityStamp()
    {
        // Arrange & Act
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());

        // Assert
        user.SecurityStamp.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Constructor_WhenCalledTwice_ShouldGenerateDistinctSecurityStamps()
    {
        // Arrange & Act
        var first = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var second = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());

        // Assert
        first.SecurityStamp.ShouldNotBe(second.SecurityStamp);
    }

    [Fact]
    public void RotateSecurityStamp_WhenCalled_ShouldChangeSecurityStamp()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var original = user.SecurityStamp;

        // Act
        user.RotateSecurityStamp();

        // Assert
        user.SecurityStamp.ShouldNotBe(original);
    }

    [Fact]
    public void AssignRole_WhenRoleNotAssigned_ShouldAddRole()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var role = new Role(_faker.Random.Word());

        // Act
        user.AssignRole(role);

        // Assert
        user.Roles.Count.ShouldBe(1);
        user.Roles.ShouldContain(role);
    }

    [Fact]
    public void AssignRole_WhenRoleDuplicated_ShouldNotAddTwice()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var role = new Role(_faker.Random.Word());

        // Act
        user.AssignRole(role);
        user.AssignRole(role);

        // Assert
        user.Roles.Count.ShouldBe(1);
    }
}
