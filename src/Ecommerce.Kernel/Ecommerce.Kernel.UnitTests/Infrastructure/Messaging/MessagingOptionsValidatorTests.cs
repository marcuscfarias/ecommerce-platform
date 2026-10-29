using Ecommerce.Kernel.Infrastructure.Messaging;
using Microsoft.Extensions.Hosting;

namespace Ecommerce.Kernel.UnitTests.Infrastructure.Messaging;

public class MessagingOptionsValidatorTests
{
    private readonly Faker _faker = new();

    [Theory]
    [InlineData("")]    // empty
    [InlineData("   ")] // whitespace only
    public void Validate_WhenConnectionStringIsMissing_ShouldFailNamingTheKey(string connectionString)
    {
        // Arrange
        var validator = CreateValidator(Environments.Development);
        var options = new MessagingOptions { RabbitMqConnectionString = connectionString };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("ConnectionStrings:RabbitMq");
    }

    [Fact]
    public void Validate_WhenProductionUsesPlainAmqp_ShouldFail()
    {
        // Arrange
        var validator = CreateValidator(Environments.Production);
        var options = new MessagingOptions
        {
            RabbitMqConnectionString = $"amqp://{_faker.Internet.UserName()}:{_faker.Internet.Password()}@{_faker.Internet.DomainName()}",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("amqps");
    }

    [Fact]
    public void Validate_WhenProductionUsesAmqps_ShouldSucceed()
    {
        // Arrange
        var validator = CreateValidator(Environments.Production);
        var options = new MessagingOptions
        {
            RabbitMqConnectionString = $"amqps://{_faker.Internet.UserName()}:{_faker.Internet.Password()}@{_faker.Internet.DomainName()}",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenDevelopmentUsesPlainAmqp_ShouldSucceed()
    {
        // Arrange
        var validator = CreateValidator(Environments.Development);
        var options = new MessagingOptions
        {
            RabbitMqConnectionString = $"amqp://{_faker.Internet.UserName()}:{_faker.Internet.Password()}@{_faker.Internet.DomainName()}",
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    private static MessagingOptionsValidator CreateValidator(string environmentName)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);

        return new MessagingOptionsValidator(environment);
    }
}
