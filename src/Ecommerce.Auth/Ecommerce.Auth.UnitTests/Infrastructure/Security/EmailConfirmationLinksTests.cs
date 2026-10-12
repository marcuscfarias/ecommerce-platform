using System.Web;
using Ecommerce.Auth.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.UnitTests.Infrastructure.Security;

public class EmailConfirmationLinksTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void ConfirmationLink_WhenApiBaseUrlHasNoTrailingSlash_ShouldBuildConfirmEmailUrl()
    {
        // Arrange
        var host = _faker.Internet.DomainName();
        var token = _faker.Random.AlphaNumeric(43);
        var sut = CreateSut(apiBaseUrl: $"https://{host}");

        // Act
        var link = sut.ConfirmationLink(token);

        // Assert
        link.AbsoluteUri.ShouldBe($"https://{host}/api/v1/auth/confirm-email?token={token}");
    }

    [Fact]
    public void ConfirmationLink_WhenApiBaseUrlHasTrailingSlash_ShouldNotDoubleTheSlash()
    {
        // Arrange
        var host = _faker.Internet.DomainName();
        var token = _faker.Random.AlphaNumeric(43);
        var sut = CreateSut(apiBaseUrl: $"https://{host}/");

        // Act
        var link = sut.ConfirmationLink(token);

        // Assert
        link.AbsoluteUri.ShouldBe($"https://{host}/api/v1/auth/confirm-email?token={token}");
    }

    [Fact]
    public void ConfirmationLink_WhenTokenHasReservedCharacters_ShouldEscapeThem()
    {
        // Arrange
        var host = _faker.Internet.DomainName();
        var sut = CreateSut(apiBaseUrl: $"https://{host}");

        // Act
        var link = sut.ConfirmationLink("a+b/c=&d");

        // Assert
        link.AbsoluteUri.ShouldBe($"https://{host}/api/v1/auth/confirm-email?token=a%2Bb%2Fc%3D%26d");
    }

    [Fact]
    public void ConfirmationLink_WhenTokenIsBase64Url_ShouldRoundTripTheToken()
    {
        // Arrange
        var token = $"{_faker.Random.AlphaNumeric(20)}-_{_faker.Random.AlphaNumeric(21)}";
        var sut = CreateSut();

        // Act
        var link = sut.ConfirmationLink(token);

        // Assert
        HttpUtility.ParseQueryString(link.Query)["token"].ShouldBe(token);
    }

    [Fact]
    public void AfterConfirmation_WhenSucceeded_ShouldPointToShopWithSuccessFlag()
    {
        // Arrange
        var host = _faker.Internet.DomainName();
        var sut = CreateSut(shopBaseUrl: $"https://{host}");

        // Act
        var target = sut.AfterConfirmation(succeeded: true);

        // Assert
        target.AbsoluteUri.ShouldBe($"https://{host}/?emailConfirmation=success");
    }

    [Fact]
    public void AfterConfirmation_WhenNotSucceeded_ShouldPointToShopWithInvalidFlag()
    {
        // Arrange
        var host = _faker.Internet.DomainName();
        var sut = CreateSut(shopBaseUrl: $"https://{host}/");

        // Act
        var target = sut.AfterConfirmation(succeeded: false);

        // Assert
        target.AbsoluteUri.ShouldBe($"https://{host}/?emailConfirmation=invalid");
    }

    [Fact]
    public void AfterConfirmation_WhenShopBaseUrlHasTrailingSlash_ShouldNotDoubleTheSlash()
    {
        // Arrange
        var host = _faker.Internet.DomainName();
        var sut = CreateSut(shopBaseUrl: $"https://{host}/");

        // Act
        var target = sut.AfterConfirmation(succeeded: true);

        // Assert
        target.AbsoluteUri.ShouldBe($"https://{host}/?emailConfirmation=success");
    }

    private EmailConfirmationLinks CreateSut(string? apiBaseUrl = null, string? shopBaseUrl = null) =>
        new(Options.Create(new EmailConfirmationSettings
        {
            ApiBaseUrl = apiBaseUrl ?? $"https://{_faker.Internet.DomainName()}",
            ShopBaseUrl = shopBaseUrl ?? $"https://{_faker.Internet.DomainName()}",
        }));
}
