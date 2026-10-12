using Ecommerce.Auth.Infrastructure.Security;

namespace Ecommerce.Auth.UnitTests.Infrastructure.Security;

public class OpaqueTokenTests
{
    [Fact]
    public void Generate_WhenCalled_ShouldReturn43CharBase64UrlString()
    {
        // Act
        var token = OpaqueToken.Generate();

        // Assert
        token.ShouldMatch("^[A-Za-z0-9_-]{43}$");
    }

    [Fact]
    public void Generate_WhenCalledTwice_ShouldReturnDifferentValues()
    {
        // Act
        var first = OpaqueToken.Generate();
        var second = OpaqueToken.Generate();

        // Assert
        first.ShouldNotBe(second);
    }

    [Fact]
    public void Hash_WhenCalled_ShouldReturnLowercaseHexSha256()
    {
        // Act
        var hash = OpaqueToken.Hash("abc");

        // Assert
        hash.ShouldBe("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
    }
}
