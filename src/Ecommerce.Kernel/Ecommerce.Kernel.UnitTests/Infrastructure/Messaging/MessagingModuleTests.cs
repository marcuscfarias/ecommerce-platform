using Ecommerce.Kernel.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.UnitTests.Infrastructure.Messaging;

public class MessagingModuleTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void AddMessaging_NoMessagingSection_ShouldUseDefaults()
    {
        // Arrange
        var provider = BuildProvider([]);

        // Act
        var options = provider.GetRequiredService<IOptions<MessagingOptions>>().Value;

        // Assert
        options.Enabled.ShouldBeFalse();
        options.Outbox.PollingInterval.ShouldBe(TimeSpan.FromSeconds(2));
        options.Outbox.BatchSize.ShouldBe(50);
        options.Outbox.MaxBackoff.ShouldBe(TimeSpan.FromSeconds(5));
        options.Outbox.PublishTimeout.ShouldBe(TimeSpan.FromSeconds(5));
        options.Consumer.RetryCount.ShouldBe(5);
        options.Consumer.RetryBaseDelay.ShouldBe(TimeSpan.FromSeconds(1));
        options.Consumer.RetryMaxDelay.ShouldBe(TimeSpan.FromSeconds(30));
        options.Consumer.PrefetchCount.ShouldBe((ushort)10);
    }

    [Fact]
    public void AddMessaging_ConnectionStringConfigured_ShouldReadItFromConnectionStrings()
    {
        // Arrange
        var connectionString = $"amqp://{_faker.Internet.DomainWord()}:5672";
        var provider = BuildProvider(new()
        {
            ["ConnectionStrings:RabbitMq"] = connectionString,
        });

        // Act
        var options = provider.GetRequiredService<IOptions<MessagingOptions>>().Value;

        // Assert
        options.ConnectionString.ShouldBe(connectionString);
    }

    [Fact]
    public void AddMessaging_DisabledWithoutConnectionString_ShouldNotFailStartup()
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            ["Messaging:Enabled"] = "false",
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    public void AddMessaging_EnabledWithoutConnectionString_ShouldFailStartupNamingTheConnectionString()
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            ["Messaging:Enabled"] = "true",
            ["ConnectionStrings:RabbitMq"] = string.Empty,
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.Throw<OptionsValidationException>(act).Message.ShouldContain("ConnectionStrings:RabbitMq");
    }

    [Theory]
    [InlineData("http://localhost:5672")] // a well-formed URI with a non-AMQP scheme
    [InlineData("localhost:5672")] // a host and port without a scheme
    public void AddMessaging_EnabledWithNonAmqpConnectionString_ShouldFailStartupNamingTheConnectionString(
        string connectionString)
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            ["Messaging:Enabled"] = "true",
            ["ConnectionStrings:RabbitMq"] = connectionString,
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.Throw<OptionsValidationException>(act).Message.ShouldContain("ConnectionStrings:RabbitMq");
    }

    [Theory]
    [InlineData("amqp://guest:guest@localhost:5672")] // plain AMQP
    [InlineData("amqps://guest:guest@localhost:5671")] // AMQP over TLS
    public void AddMessaging_EnabledWithAmqpConnectionString_ShouldNotFailStartup(string connectionString)
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            ["Messaging:Enabled"] = "true",
            ["ConnectionStrings:RabbitMq"] = connectionString,
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.NotThrow(act);
    }

    [Theory]
    [InlineData("Messaging:Outbox:PollingInterval", "00:00:00")] // zero relay polling interval
    [InlineData("Messaging:Outbox:BatchSize", "0")] // zero relay batch size
    [InlineData("Messaging:Outbox:MaxBackoff", "00:00:00")] // zero relay maximum backoff
    [InlineData("Messaging:Outbox:PublishTimeout", "00:00:00")] // zero relay publish timeout
    [InlineData("Messaging:Consumer:RetryCount", "0")] // zero consumer retries
    [InlineData("Messaging:Consumer:RetryBaseDelay", "00:00:00")] // zero consumer base retry delay
    [InlineData("Messaging:Consumer:RetryMaxDelay", "00:00:00")] // zero consumer maximum retry delay
    [InlineData("Messaging:Consumer:PrefetchCount", "0")] // zero consumer prefetch
    public void AddMessaging_TunableNotPositive_ShouldFailStartupNamingTheTunable(string key, string value)
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            [key] = value,
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        Should.Throw<OptionsValidationException>(act).Message.ShouldContain(key);
    }

    [Fact]
    public void AddMessaging_RetryBaseDelayAboveMaxDelay_ShouldFailStartupNamingBothDelays()
    {
        // Arrange
        var validator = BuildStartupValidator(new()
        {
            ["Messaging:Consumer:RetryBaseDelay"] = "00:00:31",
            ["Messaging:Consumer:RetryMaxDelay"] = "00:00:30",
        });

        // Act
        var act = () => validator.Validate();

        // Assert
        var message = Should.Throw<OptionsValidationException>(act).Message;
        message.ShouldContain("Messaging:Consumer:RetryBaseDelay");
        message.ShouldContain("Messaging:Consumer:RetryMaxDelay");
    }

    private static IStartupValidator BuildStartupValidator(Dictionary<string, string?> settings) =>
        BuildProvider(settings).GetRequiredService<IStartupValidator>();

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new ServiceCollection()
            .AddLogging()
            .AddMessaging(configuration)
            .BuildServiceProvider();
    }
}
