using Ecommerce.Auth.Infrastructure.Security;

namespace Ecommerce.Auth.UnitTests.Infrastructure.Security;

public class RefreshTokenFactoryTests
{
    private static readonly Faker Faker = new();

    private static RefreshTokenFactory CreateSut() => new();

    [Fact]
    public void Create_WhenCalled_ShouldReturnNonEmptyPlainAndHash()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var pair = sut.Create();

        // Assert
        pair.Plain.ShouldNotBeNullOrWhiteSpace();
        pair.Hash.ShouldNotBeNullOrWhiteSpace();
        pair.Hash.ShouldNotBe(pair.Plain);
    }

    [Fact]
    public void Create_WhenCalled_ShouldReturnHashMatchingItsPlain()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var pair = sut.Create();

        // Assert
        pair.Hash.ShouldBe(sut.Hash(pair.Plain));
    }

    [Fact]
    public void Create_WhenCalledTwice_ShouldReturnDifferentPlains()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var first = sut.Create();
        var second = sut.Create();

        // Assert
        first.Plain.ShouldNotBe(second.Plain);
    }

    [Fact]
    public void Hash_WhenCalledTwiceWithSamePlain_ShouldReturnSameHash()
    {
        // Arrange
        var sut = CreateSut();
        var plain = Faker.Random.Hash();

        // Act
        var first = sut.Hash(plain);
        var second = sut.Hash(plain);

        // Assert
        first.ShouldBe(second);
    }

    [Fact]
    public void Hash_WhenCalledWithDifferentPlains_ShouldReturnDifferentHashes()
    {
        // Arrange
        var sut = CreateSut();
        var first = Faker.Random.Hash();
        var second = Faker.Random.Hash();

        // Act
        var firstHash = sut.Hash(first);
        var secondHash = sut.Hash(second);

        // Assert
        firstHash.ShouldNotBe(secondHash);
    }
}
