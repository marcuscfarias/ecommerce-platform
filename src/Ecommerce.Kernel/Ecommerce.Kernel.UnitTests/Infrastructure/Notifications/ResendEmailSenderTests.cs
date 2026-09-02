using System.Globalization;
using System.Net;
using System.Text.Json;
using Ecommerce.Kernel.Application.Notifications;
using Ecommerce.Kernel.Infrastructure.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.UnitTests.Infrastructure.Notifications;

public class ResendEmailSenderTests
{
    private readonly FakeLogger<ResendEmailSender> _logger = new();
    private readonly Faker _faker = new();
    private readonly EmailOptions _options;

    public ResendEmailSenderTests()
    {
        _options = new EmailOptions
        {
            FromAddress = _faker.Internet.Email(),
            FromName = _faker.Company.CompanyName(),
        };
    }

    [Fact]
    public async Task SendAsync_WhenCalled_ShouldPostTheDocumentedRequest()
    {
        // Arrange
        var message = FakeMessage(_faker.Lorem.Sentence());
        var handler = new FakeHttpMessageHandler(Accepted());
        var sender = CreateSender(handler);

        // Act
        await sender.SendAsync(message);

        // Assert
        handler.Request!.Method.ShouldBe(HttpMethod.Post);
        handler.Request.RequestUri!.AbsolutePath.ShouldBe("/emails");

        var payload = JsonDocument.Parse(handler.RequestBody!).RootElement;
        payload.GetProperty("from").GetString().ShouldBe(_options.Sender);
        payload.GetProperty("to").GetString().ShouldBe(message.To);
        payload.GetProperty("subject").GetString().ShouldBe(message.Subject);
        payload.GetProperty("html").GetString().ShouldBe(message.HtmlBody);
        payload.GetProperty("text").GetString().ShouldBe(message.TextBody);
    }

    [Fact]
    public async Task SendAsync_WhenTextBodyIsNull_ShouldOmitTextFromThePayload()
    {
        // Arrange
        var message = FakeMessage(textBody: null);
        var handler = new FakeHttpMessageHandler(Accepted());
        var sender = CreateSender(handler);

        // Act
        await sender.SendAsync(message);

        // Assert
        var payload = JsonDocument.Parse(handler.RequestBody!).RootElement;
        payload.TryGetProperty("text", out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)] // malformed payload or unverified sending domain
    [InlineData(HttpStatusCode.TooManyRequests)] // free plan quota exhausted
    [InlineData(HttpStatusCode.InternalServerError)] // provider outage
    public async Task SendAsync_WhenResendRejects_ShouldLogWarningWithTheStatusCode(HttpStatusCode status)
    {
        // Arrange
        var message = FakeMessage(_faker.Lorem.Sentence());
        var sender = CreateSender(new FakeHttpMessageHandler(new HttpResponseMessage(status)));

        // Act
        var act = () => sender.SendAsync(message);

        // Assert
        await Should.NotThrowAsync(act);
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Warning);
        record.Message.ShouldContain(((int)status).ToString(CultureInfo.InvariantCulture));
        record.Message.ShouldNotContain(message.To);
        record.Message.ShouldNotContain(message.HtmlBody);
        record.Message.ShouldNotContain(message.TextBody!);
    }

    [Fact]
    public async Task SendAsync_WhenTheTransportThrows_ShouldLogWarningAndNotThrow()
    {
        // Arrange
        var message = FakeMessage(_faker.Lorem.Sentence());
        var transportFailure = new HttpRequestException("connection reset");
        var sender = CreateSender(new FakeHttpMessageHandler(transportFailure));

        // Act
        var act = () => sender.SendAsync(message);

        // Assert
        await Should.NotThrowAsync(act);
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Warning);
        record.Exception.ShouldBe(transportFailure);
    }

    [Fact]
    public async Task SendAsync_WhenTheRequestTimesOut_ShouldLogWarningAndNotThrow()
    {
        // Arrange
        var message = FakeMessage(_faker.Lorem.Sentence());
        var timeout = new TaskCanceledException("the request timed out", new TimeoutException());
        var sender = CreateSender(new FakeHttpMessageHandler(timeout));

        // Act
        var act = () => sender.SendAsync(message);

        // Assert
        await Should.NotThrowAsync(act);
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Warning);
        record.Exception.ShouldBe(timeout);
    }

    [Fact]
    public async Task SendAsync_WhenTheRecipientIsEmpty_ShouldLogWarningAndIssueNoRequest()
    {
        // Arrange
        var message = new EmailMessage("   ", _faker.Lorem.Sentence(), _faker.Lorem.Paragraph());
        var handler = new FakeHttpMessageHandler(Accepted());
        var sender = CreateSender(handler);

        // Act
        await sender.SendAsync(message);

        // Assert
        handler.Request.ShouldBeNull();
        _logger.Collector.GetSnapshot().ShouldHaveSingleItem().Level.ShouldBe(LogLevel.Warning);
    }

    [Fact]
    public async Task SendAsync_WhenTokenIsAlreadyCancelled_ShouldNotThrow()
    {
        // Arrange
        var message = FakeMessage(_faker.Lorem.Sentence());
        var sender = CreateSender(new FakeHttpMessageHandler(Accepted()));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () => sender.SendAsync(message, cts.Token);

        // Assert
        await Should.NotThrowAsync(act);
    }

    private ResendEmailSender CreateSender(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.resend.com") };
        return new ResendEmailSender(httpClient, Options.Create(_options), _logger);
    }

    private EmailMessage FakeMessage(string? textBody) =>
        new(_faker.Internet.Email(), _faker.Lorem.Sentence(), $"<p>{_faker.Lorem.Sentence()}</p>", textBody);

    private static HttpResponseMessage Accepted() => new(HttpStatusCode.OK);
}
