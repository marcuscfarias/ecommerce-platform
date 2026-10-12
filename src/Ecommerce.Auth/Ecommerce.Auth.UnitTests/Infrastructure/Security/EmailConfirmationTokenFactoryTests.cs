using Ecommerce.Auth.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.UnitTests.Infrastructure.Security;

public class EmailConfirmationTokenFactoryTests
{
    [Fact]
    public void Lifetime_WhenConfigured_ShouldReflectTokenLifetimeHours()
    {
        // Arrange
        var sut = CreateSut(tokenLifetimeHours: 6);

        // Act
        var lifetime = sut.Lifetime;

        // Assert
        lifetime.ShouldBe(TimeSpan.FromHours(6));
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
        pair.Hash.ShouldNotBe(pair.Plain);
    }

    private static EmailConfirmationTokenFactory CreateSut(int tokenLifetimeHours = 24) =>
        new(Options.Create(new EmailConfirmationSettings { TokenLifetimeHours = tokenLifetimeHours }));
}
