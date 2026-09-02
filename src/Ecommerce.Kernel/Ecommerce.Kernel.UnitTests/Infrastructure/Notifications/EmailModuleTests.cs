using Ecommerce.Kernel.Application.Notifications;
using Ecommerce.Kernel.Infrastructure.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.UnitTests.Infrastructure.Notifications;

public class EmailModuleTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void AddEmailSender_WhenProviderIsConsole_ShouldResolveTheConsoleSender()
    {
        // Arrange
        var provider = BuildProvider(new()
        {
            ["Email:Provider"] = "Console",
            ["Email:FromAddress"] = _faker.Internet.Email(),
        });

        // Act
        var sender = provider.CreateScope().ServiceProvider.GetRequiredService<IEmailSender>();

        // Assert
        sender.ShouldBeOfType<ConsoleEmailSender>();
    }

    [Fact]
    public void AddEmailSender_WhenProviderIsResend_ShouldResolveTheResendSender()
    {
        // Arrange
        var provider = BuildProvider(new()
        {
            ["Email:Provider"] = "Resend",
            ["Email:FromAddress"] = _faker.Internet.Email(),
            ["Email:Resend:ApiKey"] = _faker.Random.AlphaNumeric(32),
        });

        // Act
        var sender = provider.CreateScope().ServiceProvider.GetRequiredService<IEmailSender>();

        // Assert
        sender.ShouldBeOfType<ResendEmailSender>();
    }

    [Fact]
    public void AddEmailSender_WhenProviderIsAbsent_ShouldFailStartupNamingTheProvider()
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            ["Email:FromAddress"] = _faker.Internet.Email(),
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.Throw<OptionsValidationException>(act).Message.ShouldContain("Email:Provider");
    }

    [Fact]
    public void AddEmailSender_WhenProviderIsUnknown_ShouldFailStartupNamingTheProvider()
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            ["Email:Provider"] = "Sendgrid",
            ["Email:FromAddress"] = _faker.Internet.Email(),
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.Throw<Exception>(act).Message.ShouldContain("Email:Provider");
    }

    [Fact]
    public void AddEmailSender_WhenFromAddressIsEmpty_ShouldFailStartupNamingTheFromAddress()
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            ["Email:Provider"] = "Console",
            ["Email:FromAddress"] = string.Empty,
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.Throw<OptionsValidationException>(act).Message.ShouldContain("Email:FromAddress");
    }

    [Fact]
    public void AddEmailSender_WhenTheResendApiKeyIsEmpty_ShouldFailStartupNamingTheApiKey()
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            ["Email:Provider"] = "Resend",
            ["Email:FromAddress"] = _faker.Internet.Email(),
            ["Email:Resend:ApiKey"] = string.Empty,
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.Throw<OptionsValidationException>(act).Message.ShouldContain("Email:Resend:ApiKey");
    }

    [Fact]
    public void AddEmailSender_WhenProviderIsConsole_ShouldNotRequireTheResendApiKey()
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            ["Email:Provider"] = "Console",
            ["Email:FromAddress"] = _faker.Internet.Email(),
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.NotThrow(act);
    }

    private static IStartupValidator BuildStartupValidator(Dictionary<string, string?> settings) =>
        BuildProvider(settings).GetRequiredService<IStartupValidator>();

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new ServiceCollection()
            .AddLogging()
            .AddEmailSender(configuration)
            .BuildServiceProvider();
    }
}
