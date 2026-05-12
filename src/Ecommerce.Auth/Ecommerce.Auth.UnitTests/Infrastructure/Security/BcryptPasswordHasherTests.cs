using Ecommerce.Auth.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.UnitTests.Infrastructure.Security;

public class BcryptPasswordHasherTests
{
    private static BcryptPasswordHasher CreateSut(int workFactor = 4)
    {
        var options = Options.Create(new AuthPasswordSettings { BcryptWorkFactor = workFactor });
        return new BcryptPasswordHasher(options);
    }

    [Fact]
    public void Hash_WhenCalled_ShouldReturnNonEmptyValue()
    {
        // Arrange
        var sut = CreateSut();
        var plain = "Str0ngP@ssword!";

        // Act
        var hash = sut.Hash(plain);

        // Assert
        hash.ShouldNotBeNullOrWhiteSpace();
        hash.ShouldNotBe(plain);
    }

    [Fact]
    public void Hash_WhenCalledTwiceWithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var sut = CreateSut();
        var plain = "Str0ngP@ssword!";

        // Act
        var first = sut.Hash(plain);
        var second = sut.Hash(plain);

        // Assert
        first.ShouldNotBe(second);
    }

    [Fact]
    public void Verify_WhenHashMatchesPlain_ShouldReturnTrue()
    {
        // Arrange
        var sut = CreateSut();
        var plain = "Str0ngP@ssword!";
        var hash = sut.Hash(plain);

        // Act
        var result = sut.Verify(plain, hash);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Verify_WhenHashDiffersFromPlain_ShouldReturnFalse()
    {
        // Arrange
        var sut = CreateSut();
        var hash = sut.Hash("Str0ngP@ssword!");

        // Act
        var result = sut.Verify("DifferentPassword!", hash);

        // Assert
        result.ShouldBeFalse();
    }
}
