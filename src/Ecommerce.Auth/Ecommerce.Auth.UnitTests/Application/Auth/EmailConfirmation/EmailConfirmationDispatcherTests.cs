using System.Globalization;
using Ecommerce.Auth.Application.Auth.EmailConfirmation;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Auth.UnitTests.Application.Auth.EmailConfirmation;

public class EmailConfirmationDispatcherTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IEmailConfirmationTokenFactory _tokenFactory = Substitute.For<IEmailConfirmationTokenFactory>();
    private readonly IEmailConfirmationLinks _links = Substitute.For<IEmailConfirmationLinks>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly FakeLogger<EmailConfirmationDispatcher> _logger = new();
    private readonly Faker _faker = new();
    private readonly EmailConfirmationTokenPair _tokenPair;
    private readonly TimeSpan _lifetime;
    private readonly Uri _link;
    private readonly EmailConfirmationDispatcher _dispatcher;

    public EmailConfirmationDispatcherTests()
    {
        _tokenPair = new EmailConfirmationTokenPair(_faker.Random.AlphaNumeric(43), _faker.Random.Hash());
        _lifetime = TimeSpan.FromHours(_faker.Random.Int(1, 72));
        _link = new Uri($"https://{_faker.Internet.DomainName()}/api/v1/auth/confirm-email?token={_tokenPair.Plain}");

        _tokenFactory.Create().Returns(_tokenPair);
        _tokenFactory.Lifetime.Returns(_lifetime);
        _links.ConfirmationLink(_tokenPair.Plain).Returns(_link);
        _repository.GetActiveUserTokensAsync(default, default, default).ReturnsForAnyArgs([]);

        _dispatcher = new EmailConfirmationDispatcher(
            _repository, _tokenFactory, _links, _emailSender, _timeProvider, _logger);
    }

    [Fact]
    public async Task DispatchAsync_WhenUserHasActiveTokens_ShouldRevokeEveryActiveToken()
    {
        // Arrange
        var user = FakeUser();
        var now = _timeProvider.GetUtcNow();
        UserToken[] activeTokens = [FakeActiveToken(user, now), FakeActiveToken(user, now)];

        _repository
            .GetActiveUserTokensAsync(user.Id, UserTokenPurpose.EmailConfirmation, now, Arg.Any<CancellationToken>())
            .Returns(activeTokens);

        // Act
        await _dispatcher.DispatchAsync(user, CancellationToken.None);

        // Assert
        activeTokens.ShouldAllBe(t => t.RevokedAt == now);
    }

    [Fact]
    public async Task DispatchAsync_WhenCalled_ShouldAddOneTokenWithHashPurposeAndExpiry()
    {
        // Arrange
        var user = FakeUser();
        var now = _timeProvider.GetUtcNow();
        UserToken? added = null;
        _repository.AddUserToken(Arg.Do<UserToken>(t => added = t));

        // Act
        await _dispatcher.DispatchAsync(user, CancellationToken.None);

        // Assert
        _repository.Received(1).AddUserToken(Arg.Any<UserToken>());
        added.ShouldNotBeNull();
        added.UserId.ShouldBe(user.Id);
        added.Purpose.ShouldBe(UserTokenPurpose.EmailConfirmation);
        added.TokenHash.ShouldBe(_tokenPair.Hash);
        added.ExpiresAt.ShouldBe(now + _lifetime);
        added.CreatedAt.ShouldBe(now);
        added.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public async Task DispatchAsync_WhenCalled_ShouldSaveBeforeSending()
    {
        // Arrange
        var user = FakeUser();

        // Act
        await _dispatcher.DispatchAsync(user, CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            _repository.SaveChangesAsync(Arg.Any<CancellationToken>());
            _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task DispatchAsync_WhenCalled_ShouldSendOneEmailToTheUserWithTheLink()
    {
        // Arrange
        var user = FakeUser();
        EmailMessage? sent = null;
        _emailSender
            .SendAsync(Arg.Do<EmailMessage>(m => sent = m), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _dispatcher.DispatchAsync(user, CancellationToken.None);

        // Assert
        await _emailSender.Received(1).SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        sent.ShouldNotBeNull();
        sent.To.ShouldBe(user.Email);
        sent.HtmlBody.ShouldContain(_link.AbsoluteUri);
        sent.TextBody.ShouldNotBeNull().ShouldContain(_link.AbsoluteUri);
    }

    [Fact]
    public async Task DispatchAsync_WhenCalled_ShouldLogOneInformationEntryWithUserIdAndNoSensitiveData()
    {
        // Arrange
        var user = FakeUser();

        // Act
        await _dispatcher.DispatchAsync(user, CancellationToken.None);

        // Assert
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.StructuredState.ShouldNotBeNull()
            .ShouldContain(new KeyValuePair<string, string?>("UserId", user.Id.ToString(CultureInfo.InvariantCulture)));
        record.Message.ShouldNotContain(user.Email);
        record.Message.ShouldNotContain(_tokenPair.Plain);
        record.Message.ShouldNotContain(_link.AbsoluteUri);
    }

    private User FakeUser() =>
        new(_faker.Internet.Email(), _faker.Random.Hash(), _faker.Person.FullName);

    private UserToken FakeActiveToken(User user, DateTimeOffset now) =>
        new(user.Id, UserTokenPurpose.EmailConfirmation, _faker.Random.Hash(), now.AddHours(1), now.AddHours(-1));
}
