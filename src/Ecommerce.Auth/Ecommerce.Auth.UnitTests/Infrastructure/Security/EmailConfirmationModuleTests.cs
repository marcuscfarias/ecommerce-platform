using Ecommerce.Auth.Application.Auth.EmailConfirmation;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.UnitTests.Infrastructure.Security;

public class EmailConfirmationModuleTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void AddEmailConfirmation_WhenSettingsAreValid_ShouldResolveTheTokenFactoryWithConfiguredLifetime()
    {
        // Arrange
        var settings = ValidSettings();
        settings["Auth:EmailConfirmation:TokenLifetimeHours"] = "6";
        var provider = BuildProvider(settings);

        // Act
        var factory = provider.GetRequiredService<IEmailConfirmationTokenFactory>();

        // Assert
        factory.Lifetime.ShouldBe(TimeSpan.FromHours(6));
    }

    [Fact]
    public void AddEmailConfirmation_WhenSettingsAreValid_ShouldResolveLinksFromConfiguredShopBaseUrl()
    {
        // Arrange
        var settings = ValidSettings();
        var shopHost = _faker.Internet.DomainName();
        settings["Auth:EmailConfirmation:ShopBaseUrl"] = $"https://{shopHost}";
        var provider = BuildProvider(settings);

        // Act
        var links = provider.GetRequiredService<IEmailConfirmationLinks>();

        // Assert
        links.AfterConfirmation(succeeded: true).AbsoluteUri.ShouldBe($"https://{shopHost}/?emailConfirmation=success");
    }

    [Theory]
    [InlineData(null)] // absent
    [InlineData("   ")] // whitespace only
    [InlineData("/confirm")] // relative path
    [InlineData("ftp://api.example.com")] // non-http scheme
    public void AddEmailConfirmation_WhenApiBaseUrlIsInvalid_ShouldFailStartupNamingTheSetting(string? apiBaseUrl)
    {
        // Arrange
        var settings = ValidSettings();
        settings["Auth:EmailConfirmation:ApiBaseUrl"] = apiBaseUrl;
        var validator = BuildProvider(settings).GetRequiredService<IStartupValidator>();

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.Throw<OptionsValidationException>(act).Message.ShouldContain("Auth:EmailConfirmation:ApiBaseUrl");
    }

    [Theory]
    [InlineData(null)] // absent
    [InlineData("   ")] // whitespace only
    [InlineData("/shop")] // relative path
    [InlineData("ftp://shop.example.com")] // non-http scheme
    public void AddEmailConfirmation_WhenShopBaseUrlIsInvalid_ShouldFailStartupNamingTheSetting(string? shopBaseUrl)
    {
        // Arrange
        var settings = ValidSettings();
        settings["Auth:EmailConfirmation:ShopBaseUrl"] = shopBaseUrl;
        var validator = BuildProvider(settings).GetRequiredService<IStartupValidator>();

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.Throw<OptionsValidationException>(act).Message.ShouldContain("Auth:EmailConfirmation:ShopBaseUrl");
    }

    [Theory]
    [InlineData("0")] // zero
    [InlineData("-1")] // negative
    public void AddEmailConfirmation_WhenTokenLifetimeHoursIsNotPositive_ShouldFailStartupNamingTheSetting(string lifetimeHours)
    {
        // Arrange
        var settings = ValidSettings();
        settings["Auth:EmailConfirmation:TokenLifetimeHours"] = lifetimeHours;
        var validator = BuildProvider(settings).GetRequiredService<IStartupValidator>();

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.Throw<OptionsValidationException>(act).Message.ShouldContain("Auth:EmailConfirmation:TokenLifetimeHours");
    }

    private Dictionary<string, string?> ValidSettings() => new()
    {
        ["Auth:EmailConfirmation:ApiBaseUrl"] = $"https://{_faker.Internet.DomainName()}",
        ["Auth:EmailConfirmation:ShopBaseUrl"] = $"https://{_faker.Internet.DomainName()}",
    };

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new ServiceCollection()
            .AddEmailConfirmation(configuration)
            .BuildServiceProvider();
    }
}
