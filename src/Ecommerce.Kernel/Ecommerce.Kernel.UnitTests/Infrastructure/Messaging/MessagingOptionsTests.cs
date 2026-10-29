using Ecommerce.Kernel.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.UnitTests.Infrastructure.Messaging;

public class MessagingOptionsTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void RetryCooldowns_WhenNotConfigured_ShouldGrowFromOneToThirtySeconds()
    {
        // Arrange
        var provider = BuildProvider(new()
        {
            ["ConnectionStrings:RabbitMq"] = $"amqp://{_faker.Internet.DomainName()}",
        });

        // Act
        var options = provider.GetRequiredService<IOptions<MessagingOptions>>().Value;

        // Assert
        options.RetryCooldowns.ShouldBe(
        [
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(12),
            TimeSpan.FromSeconds(30),
        ]);
    }

    [Fact]
    public void RetryCooldowns_WhenConfigured_ShouldReplaceTheDefaults()
    {
        // Arrange
        var provider = BuildProvider(new()
        {
            ["ConnectionStrings:RabbitMq"] = $"amqp://{_faker.Internet.DomainName()}",
            ["Messaging:RetryCooldowns:0"] = "00:00:00.010",
            ["Messaging:RetryCooldowns:1"] = "00:00:00.020",
        });

        // Act
        var options = provider.GetRequiredService<IOptions<MessagingOptions>>().Value;

        // Assert
        options.RetryCooldowns.ShouldBe([TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20)]);
    }

    [Fact]
    public void DuplicateDetectionWindow_WhenNotConfigured_ShouldBeTwentyFourHours()
    {
        // Arrange
        var provider = BuildProvider(new()
        {
            ["ConnectionStrings:RabbitMq"] = $"amqp://{_faker.Internet.DomainName()}",
        });

        // Act
        var options = provider.GetRequiredService<IOptions<MessagingOptions>>().Value;

        // Assert
        options.DuplicateDetectionWindow.ShouldBe(TimeSpan.FromHours(24));
    }

    [Fact]
    public void RabbitMqConnectionString_WhenConnectionStringIsSet_ShouldBeReadFromConnectionStrings()
    {
        // Arrange
        var connectionString = $"amqp://{_faker.Internet.DomainName()}";
        var provider = BuildProvider(new()
        {
            ["ConnectionStrings:RabbitMq"] = connectionString,
        });

        // Act
        var options = provider.GetRequiredService<IOptions<MessagingOptions>>().Value;

        // Assert
        options.RabbitMqConnectionString.ShouldBe(connectionString);
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(environment)
            .AddMessagingOptions()
            .BuildServiceProvider();
    }
}
