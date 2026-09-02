using Ecommerce.Kernel.Application.Notifications;
using Ecommerce.Kernel.Infrastructure.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.UnitTests.Infrastructure.Notifications;

public class ConsoleEmailSenderTests
{
    private readonly FakeLogger<ConsoleEmailSender> _logger = new();
    private readonly Faker _faker = new();
    private readonly EmailOptions _options;
    private readonly ConsoleEmailSender _sender;

    public ConsoleEmailSenderTests()
    {
        _options = new EmailOptions
        {
            FromAddress = _faker.Internet.Email(),
            FromName = _faker.Company.CompanyName(),
        };

        _sender = new ConsoleEmailSender(Options.Create(_options), _logger);
    }

    [Fact]
    public async Task SendAsync_WhenCalled_ShouldLogTheWholeMessageAtInformation()
    {
        // Arrange
        var message = FakeMessage();

        // Act
        await _sender.SendAsync(message);

        // Assert
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.Message.ShouldContain(_options.Sender);
        record.Message.ShouldContain(message.To);
        record.Message.ShouldContain(message.Subject);
        record.Message.ShouldContain(message.HtmlBody);
    }

    [Fact]
    public async Task SendAsync_WhenTokenIsAlreadyCancelled_ShouldNotThrow()
    {
        // Arrange
        var message = FakeMessage();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => _sender.SendAsync(message, cts.Token);

        // Assert
        await Should.NotThrowAsync(act);
    }

    private EmailMessage FakeMessage() =>
        new(_faker.Internet.Email(), _faker.Lorem.Sentence(), $"<p>{_faker.Lorem.Sentence()}</p>");
}
