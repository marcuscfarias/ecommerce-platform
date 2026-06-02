using Ecommerce.Auth.Domain.Entities;

namespace Ecommerce.Auth.UnitTests.Domain.Entities;

public class RefreshTokenTests
{
    private readonly Faker _faker = new();

    private RefreshToken CreateToken(DateTimeOffset expiresAt, DateTimeOffset createdAt) =>
        new(_faker.Random.Int(1), _faker.Random.Hash(), expiresAt, _faker.Random.Hash(), createdAt);

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var token = CreateToken(expiresAt: now.AddDays(7), createdAt: now);

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
        var token = CreateToken(expiresAt: now.AddSeconds(-1), createdAt: now.AddDays(-7));

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
        var token = CreateToken(expiresAt: now.AddDays(7), createdAt: now);
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
        var token = CreateToken(expiresAt: now.AddDays(7), createdAt: now);

        // Act
        token.Revoke(now);

        // Assert
        token.RevokedAt.ShouldBe(now);
    }
}
