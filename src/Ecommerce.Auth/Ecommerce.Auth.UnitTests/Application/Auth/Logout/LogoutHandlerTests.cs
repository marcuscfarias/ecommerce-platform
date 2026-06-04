using Ecommerce.Auth.Application.Auth.Logout;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Repositories;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Auth.UnitTests.Application.Auth.Logout;

public class LogoutHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IRefreshTokenFactory _refreshTokenFactory = Substitute.For<IRefreshTokenFactory>();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly Faker _faker = new();
    private readonly LogoutHandler _handler;

    public LogoutHandlerTests()
    {
        _handler = new LogoutHandler(_repository, _refreshTokenFactory, _timeProvider);
    }

    [Fact]
    public async Task Handle_WhenTokenIsActive_ShouldRevokeAndSave()
    {
        // Arrange
        var command = FakeCommand();
        var hash = _faker.Random.Hash();

        var now = _faker.Date.RecentOffset();
        _timeProvider.SetUtcNow(now);
        var token = FakeRefreshToken(now.AddDays(7), now.AddDays(-1));

        _refreshTokenFactory.Hash(command.RefreshToken).Returns(hash);
        _repository.GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>()).Returns(token);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        token.RevokedAt.ShouldBe(now);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTokenIsEmpty_ShouldDoNothing()
    {
        // Arrange
        var command = new LogoutCommand(string.Empty);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await Should.NotThrowAsync(act);
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ShouldDoNothing()
    {
        // Arrange
        var command = FakeCommand();
        var hash = _faker.Random.Hash();

        _refreshTokenFactory.Hash(command.RefreshToken).Returns(hash);
        _repository.GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTokenIsAlreadyRevoked_ShouldDoNothing()
    {
        // Arrange
        var command = FakeCommand();
        var hash = _faker.Random.Hash();

        var now = _faker.Date.RecentOffset();
        _timeProvider.SetUtcNow(now);
        var token = FakeRefreshToken(now.AddDays(7), now.AddDays(-1));
        token.Revoke(now.AddDays(-1));
        var firstRevocation = token.RevokedAt;

        _refreshTokenFactory.Hash(command.RefreshToken).Returns(hash);
        _repository.GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>()).Returns(token);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        token.RevokedAt.ShouldBe(firstRevocation);
    }

    private LogoutCommand FakeCommand() => new(_faker.Random.AlphaNumeric(43));

    private RefreshToken FakeRefreshToken(DateTimeOffset expiresAt, DateTimeOffset createdAt) =>
        new(_faker.Random.Int(), _faker.Random.Hash(), expiresAt, _faker.Random.Hash(), createdAt);
}
