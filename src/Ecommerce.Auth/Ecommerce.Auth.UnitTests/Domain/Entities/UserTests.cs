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
    public void Deactivate_WhenCalled_ShouldRotateSecurityStamp()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var original = user.SecurityStamp;

        // Act
        user.Deactivate();

        // Assert
        user.SecurityStamp.ShouldNotBe(original);
    }

    [Fact]
    public void Reactivate_WhenUserIsInactive_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName(), isActive: false);

        // Act
        user.Reactivate();

        // Assert
        user.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void ResetPassword_WhenCalled_ShouldChangePasswordHashAndRotateSecurityStamp()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var originalStamp = user.SecurityStamp;
        var newHash = _faker.Random.AlphaNumeric(60);

        // Act
        user.ResetPassword(newHash);

        // Assert
        user.PasswordHash.ShouldBe(newHash);
        user.SecurityStamp.ShouldNotBe(originalStamp);
    }

    [Fact]
    public void SetRoles_WhenUserHasExistingRoles_ShouldReplaceThem()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        user.AssignRole(new Role(_faker.Random.Word()));
        var newRole = new Role(_faker.Random.Word());

        // Act
        user.SetRoles([newRole]);

        // Assert
        user.Roles.Count.ShouldBe(1);
        user.Roles.ShouldContain(newRole);
    }

    [Fact]
    public void SetRoles_WhenSameRoleInstanceRepeated_ShouldDeduplicate()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var role = new Role(_faker.Random.Word());

        // Act
        user.SetRoles([role, role]);

        // Assert
        user.Roles.Count.ShouldBe(1);
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
    public void RegisterFailedAccess_WhenBelowThreshold_ShouldIncrementWithoutLocking()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var now = DateTimeOffset.UtcNow;

        // Act
        user.RegisterFailedAccess(now, maxAttempts: 5, lockoutDuration: TimeSpan.FromMinutes(15));

        // Assert
        user.AccessFailedCount.ShouldBe(1);
        user.LockoutEnd.ShouldBeNull();
        user.IsLockedOut(now).ShouldBeFalse();
    }

    [Fact]
    public void RegisterFailedAccess_WhenThresholdReached_ShouldLockAndResetCounter()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var now = DateTimeOffset.UtcNow;
        var lockoutDuration = TimeSpan.FromMinutes(15);

        // Act
        for (var attempt = 0; attempt < 5; attempt++)
            user.RegisterFailedAccess(now, maxAttempts: 5, lockoutDuration);

        // Assert
        user.AccessFailedCount.ShouldBe(0);
        user.LockoutEnd.ShouldBe(now + lockoutDuration);
    }

    [Fact]
    public void IsLockedOut_WhenWithinLockoutWindow_ShouldBeTrue()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var now = DateTimeOffset.UtcNow;
        user.RegisterFailedAccess(now, maxAttempts: 1, lockoutDuration: TimeSpan.FromMinutes(15));

        // Act
        var locked = user.IsLockedOut(now + TimeSpan.FromMinutes(5));

        // Assert
        locked.ShouldBeTrue();
    }

    [Fact]
    public void IsLockedOut_WhenLockoutWindowElapsed_ShouldBeFalse()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var now = DateTimeOffset.UtcNow;
        user.RegisterFailedAccess(now, maxAttempts: 1, lockoutDuration: TimeSpan.FromMinutes(15));

        // Act
        var locked = user.IsLockedOut(now + TimeSpan.FromMinutes(20));

        // Assert
        locked.ShouldBeFalse();
    }

    [Fact]
    public void ResetAccessFailedCount_WhenCalled_ShouldClearCounterAndLockout()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.AlphaNumeric(60), _faker.Name.FullName());
        var now = DateTimeOffset.UtcNow;
        user.RegisterFailedAccess(now, maxAttempts: 1, lockoutDuration: TimeSpan.FromMinutes(15));

        // Act
        user.ResetAccessFailedCount();

        // Assert
        user.AccessFailedCount.ShouldBe(0);
        user.LockoutEnd.ShouldBeNull();
        user.IsLockedOut(now).ShouldBeFalse();
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
