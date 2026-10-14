using System.Globalization;
using Ecommerce.Auth.Application.Auth.ConfirmEmail;
using Ecommerce.Auth.Application.Auth.EmailConfirmation;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Auth.UnitTests.Application.Auth.ConfirmEmail;

public class ConfirmEmailHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IEmailConfirmationTokenFactory _tokenFactory = Substitute.For<IEmailConfirmationTokenFactory>();
    private readonly IEmailConfirmationLinks _links = Substitute.For<IEmailConfirmationLinks>();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly FakeLogger<ConfirmEmailHandler> _logger = new();
    private readonly Faker _faker = new();
    private readonly Uri _successUri;
    private readonly Uri _invalidUri;
    private readonly ConfirmEmailCommand _command;
    private readonly string _hash;
    private readonly ConfirmEmailHandler _handler;

    public ConfirmEmailHandlerTests()
    {
        var shop = _faker.Internet.DomainName();
        _successUri = new Uri($"https://{shop}/?emailConfirmation=success");
        _invalidUri = new Uri($"https://{shop}/?emailConfirmation=invalid");
        _command = new ConfirmEmailCommand(_faker.Random.AlphaNumeric(43));
        _hash = _faker.Random.Hash();

        _links.AfterConfirmation(true).Returns(_successUri);
        _links.AfterConfirmation(false).Returns(_invalidUri);
        _tokenFactory.Hash(_command.Token).Returns(_hash);

        _handler = new ConfirmEmailHandler(_repository, _tokenFactory, _links, _timeProvider, _logger);
    }

    [Theory]
    [InlineData("")]    // empty query value
    [InlineData("   ")] // whitespace only
    public async Task Handle_WhenTokenIsEmpty_ShouldRedirectToInvalid(string token)
    {
        // Act
        var result = await _handler.Handle(new ConfirmEmailCommand(token), CancellationToken.None);

        // Assert
        result.RedirectTo.ShouldBe(_invalidUri);
    }

    [Fact]
    public async Task Handle_WhenHashIsUnknown_ShouldRedirectToInvalid()
    {
        // Arrange
        _repository.GetUserTokenByHashAsync(_hash, UserTokenPurpose.EmailConfirmation, Arg.Any<CancellationToken>())
            .Returns((UserToken?)null);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.RedirectTo.ShouldBe(_invalidUri);
    }

    [Fact]
    public async Task Handle_WhenTokenIsActive_ShouldConfirmUserRevokeTokenAndRedirectToSuccess()
    {
        // Arrange
        var now = _timeProvider.GetUtcNow();
        var user = FakeUser(emailConfirmed: false);
        var token = FakeToken(user, expiresAt: now.AddHours(1));
        ArrangeLookup(user, token);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.RedirectTo.ShouldBe(_successUri);
        user.EmailConfirmed.ShouldBeTrue();
        token.RevokedAt.ShouldBe(now);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]  // revoked token
    [InlineData(false)] // expired token
    public async Task Handle_WhenTokenIsInactiveAndUserIsConfirmed_ShouldRedirectToSuccess(bool revoked)
    {
        // Arrange
        var user = FakeUser(emailConfirmed: true);
        var token = FakeInactiveToken(user, revoked);
        var originalRevokedAt = token.RevokedAt;
        ArrangeLookup(user, token);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.RedirectTo.ShouldBe(_successUri);
        token.RevokedAt.ShouldBe(originalRevokedAt);
        user.EmailConfirmed.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserIsInactiveAndTokenIsActive_ShouldConfirmUserAndRedirectToSuccess()
    {
        // Arrange
        var user = FakeUser(emailConfirmed: false, isActive: false);
        ArrangeLookup(user, FakeToken(user, expiresAt: _timeProvider.GetUtcNow().AddHours(1)));

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.RedirectTo.ShouldBe(_successUri);
        user.EmailConfirmed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true)]  // revoked token
    [InlineData(false)] // expired token
    public async Task Handle_WhenTokenIsInactiveAndUserIsPending_ShouldRedirectToInvalidAndKeepUserPending(bool revoked)
    {
        // Arrange
        var user = FakeUser(emailConfirmed: false);
        var token = FakeInactiveToken(user, revoked);
        ArrangeLookup(user, token);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.RedirectTo.ShouldBe(_invalidUri);
        user.EmailConfirmed.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WhenTokenIsActive_ShouldLogEmailConfirmedWithUserIdAndNoToken()
    {
        // Arrange
        var user = FakeUser(emailConfirmed: false);
        ArrangeLookup(user, FakeToken(user, expiresAt: _timeProvider.GetUtcNow().AddHours(1)));

        // Act
        await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.StructuredState.ShouldNotBeNull()
            .ShouldContain(new KeyValuePair<string, string?>("UserId", user.Id.ToString(CultureInfo.InvariantCulture)));
        record.Message.ShouldNotContain(_command.Token);
        record.Message.ShouldNotContain(user.Email);
    }

    [Theory]
    [InlineData("empty-token")]          // token query value is empty
    [InlineData("unknown-hash")]         // hash matches no stored token
    [InlineData("missing-user")]         // token points at a user that no longer exists
    [InlineData("inactive-for-pending")] // expired token of a user still pending
    public async Task Handle_WhenTokenIsRejected_ShouldLogOneEntryWithoutUserIdOrToken(string rejection)
    {
        // Arrange
        var command = rejection == "empty-token" ? new ConfirmEmailCommand("") : _command;
        var user = FakeUser(emailConfirmed: false);
        switch (rejection)
        {
            case "unknown-hash":
                _repository.GetUserTokenByHashAsync(_hash, UserTokenPurpose.EmailConfirmation, Arg.Any<CancellationToken>())
                    .Returns((UserToken?)null);
                break;
            case "missing-user":
                _repository.GetUserTokenByHashAsync(_hash, UserTokenPurpose.EmailConfirmation, Arg.Any<CancellationToken>())
                    .Returns(FakeToken(user, expiresAt: _timeProvider.GetUtcNow().AddHours(1)));
                _repository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns((User?)null);
                break;
            case "inactive-for-pending":
                ArrangeLookup(user, FakeInactiveToken(user, revoked: false));
                break;
        }

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.StructuredState.ShouldNotBeNull().ShouldNotContain(e => e.Key == "UserId");
        record.Message.ShouldNotContain(_command.Token);
        record.Message.ShouldNotContain(user.Email);
    }

    private void ArrangeLookup(User user, UserToken token)
    {
        _repository.GetUserTokenByHashAsync(_hash, UserTokenPurpose.EmailConfirmation, Arg.Any<CancellationToken>())
            .Returns(token);
        _repository.GetByIdAsync(token.UserId, Arg.Any<CancellationToken>()).Returns(user);
    }

    private User FakeUser(bool emailConfirmed, bool isActive = true) =>
        new(_faker.Internet.Email(), _faker.Random.Hash(), _faker.Person.FullName, isActive, emailConfirmed);

    private UserToken FakeToken(User user, DateTimeOffset expiresAt) =>
        new(user.Id, UserTokenPurpose.EmailConfirmation, _hash, expiresAt, _timeProvider.GetUtcNow().AddHours(-1));

    private UserToken FakeInactiveToken(User user, bool revoked)
    {
        var now = _timeProvider.GetUtcNow();
        var token = FakeToken(user, expiresAt: revoked ? now.AddHours(1) : now.AddMinutes(-1));
        if (revoked)
            token.Revoke(now.AddMinutes(-1));
        return token;
    }
}
