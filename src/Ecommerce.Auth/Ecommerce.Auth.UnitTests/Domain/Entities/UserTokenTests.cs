using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;

namespace Ecommerce.Auth.UnitTests.Domain.Entities;

public class UserTokenTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var token = CreateToken(expiresAt: now.AddHours(24), createdAt: now);

        // Act
        var result = token.IsActive(now);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var token = CreateToken(expiresAt: now.AddSeconds(-1), createdAt: now.AddHours(-24));

        // Act
        var result = token.IsActive(now);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsActive_WhenNowEqualsExpiresAt_ShouldReturnFalse()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var token = CreateToken(expiresAt: now, createdAt: now.AddHours(-24));

        // Act
        var result = token.IsActive(now);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var token = CreateToken(expiresAt: now.AddHours(24), createdAt: now);
        token.Revoke(now);

        // Act
        var result = token.IsActive(now);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Revoke_WhenCalled_ShouldSetRevokedAt()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var token = CreateToken(expiresAt: now.AddHours(24), createdAt: now);

        // Act
        token.Revoke(now);

        // Assert
        token.RevokedAt.ShouldBe(now);
    }

    private UserToken CreateToken(DateTimeOffset expiresAt, DateTimeOffset createdAt) =>
        new(_faker.Random.Int(1), UserTokenPurpose.EmailConfirmation, _faker.Random.Hash(), expiresAt, createdAt);
}
