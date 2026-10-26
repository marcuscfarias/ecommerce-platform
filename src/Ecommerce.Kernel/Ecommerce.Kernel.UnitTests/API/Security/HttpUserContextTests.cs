using System.Globalization;
using System.Security.Claims;
using Ecommerce.Kernel.API.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Ecommerce.Kernel.UnitTests.API.Security;

public class HttpUserContextTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Name_WhenNameClaimExists_ShouldReturnItOverTheEmail()
    {
        // Arrange
        var name = _faker.Name.FullName();
        var sut = CreateSut(
            new Claim(JwtRegisteredClaimNames.Name, name),
            new Claim(JwtRegisteredClaimNames.Email, _faker.Internet.Email()));

        // Act
        var result = sut.Name;

        // Assert
        result.ShouldBe(name);
    }

    [Fact]
    public void Name_WhenOnlyNameClaimExists_ShouldReturnIt()
    {
        // Arrange
        var name = _faker.Name.FullName();
        var sut = CreateSut(new Claim(JwtRegisteredClaimNames.Name, name));

        // Act
        var result = sut.Name;

        // Assert
        result.ShouldBe(name);
    }

    [Fact]
    public void Name_WhenNameClaimIsMissing_ShouldReturnTheEmailLocalPart()
    {
        // Arrange
        var localPart = _faker.Internet.UserName();
        var sut = CreateSut(new Claim(JwtRegisteredClaimNames.Email, $"{localPart}@{_faker.Internet.DomainName()}"));

        // Act
        var result = sut.Name;

        // Assert
        result.ShouldBe(localPart);
    }

    [Fact]
    public void Name_WhenNameAndEmailClaimsAreMissing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var sut = CreateSut(new Claim(JwtRegisteredClaimNames.Sub, _faker.Random.Int(1).ToString(CultureInfo.InvariantCulture)));

        // Act
        var act = () => sut.Name;

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    private static HttpUserContext CreateSut(params Claim[] claims)
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) },
        };

        return new HttpUserContext(accessor);
    }
}
