using Ecommerce.Kernel.Infrastructure.Notifications;

namespace Ecommerce.Kernel.UnitTests.Infrastructure.Notifications;

public class EmailOptionsTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Sender_WhenFromNameIsSet_ShouldFormatNameAndAddress()
    {
        // Arrange
        var fromName = _faker.Company.CompanyName();
        var fromAddress = _faker.Internet.Email();
        var options = new EmailOptions { FromAddress = fromAddress, FromName = fromName };

        // Act
        var sender = options.Sender;

        // Assert
        sender.ShouldBe($"{fromName} <{fromAddress}>");
    }

    [Fact]
    public void Sender_WhenFromNameIsEmpty_ShouldReturnBareAddress()
    {
        // Arrange
        var fromAddress = _faker.Internet.Email();
        var options = new EmailOptions { FromAddress = fromAddress, FromName = string.Empty };

        // Act
        var sender = options.Sender;

        // Assert
        sender.ShouldBe(fromAddress);
    }
}
