using Ecommerce.Auth.Application.Auth.Refresh;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Application.Exceptions;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Auth.UnitTests.Application.Auth.Refresh;

public class RefreshHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IRefreshTokenFactory _refreshTokenFactory = Substitute.For<IRefreshTokenFactory>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly Faker _faker = new();
    private readonly RefreshHandler _handler;

    public RefreshHandlerTests()
    {
        _handler = new RefreshHandler(_repository, _refreshTokenFactory, _jwtTokenGenerator, _timeProvider);
    }

    [Fact]
    public async Task Handle_WhenTokenIsValid_ShouldReturnNewAccessToken()
    {
        // Arrange
        var command = FakeCommand();
        var hash = _faker.Random.Hash();

        var user = FakeUser(isActive: true);

        var issuedAt = _faker.Date.RecentOffset();
        _timeProvider.SetUtcNow(issuedAt);
        var tokenCreation = _timeProvider.GetUtcNow();
        var tokenExpiration = tokenCreation.AddDays(7);
        var validToken = FakeRefreshToken(tokenExpiration, tokenCreation, user.SecurityStamp);
        var accessToken = new JwtAccessToken(_faker.Random.AlphaNumeric(32), _faker.Random.Int(60, 3600));

        _refreshTokenFactory.Hash(command.RefreshToken).Returns(hash);
        _repository.GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>()).Returns(validToken);
        _repository.GetByIdWithRolesAsync(validToken.UserId, Arg.Any<CancellationToken>()).Returns(user);
        _jwtTokenGenerator.Generate(user.Id, user.Email, Arg.Any<IEnumerable<RoleName>>()).Returns(accessToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.ShouldBe(accessToken.Token);
        result.AccessTokenExpiresInSeconds.ShouldBe(accessToken.ExpiresInSeconds);
        _jwtTokenGenerator.Received(1).Generate(user.Id, user.Email, Arg.Any<IEnumerable<RoleName>>());
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ShouldThrowInvalidRefreshToken()
    {
        // Arrange
        var command = FakeCommand();
        var hash = _faker.Random.Hash();

        _refreshTokenFactory.Hash(command.RefreshToken).Returns(hash);
        _repository.GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>()).Returns((RefreshToken?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidRefreshTokenException>();
        _refreshTokenFactory.Received(1).Hash(command.RefreshToken);
        await _repository.Received(1).GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTokenIsExpired_ShouldThrowInvalidRefreshToken()
    {
        // Arrange
        var command = FakeCommand();
        var hash = _faker.Random.Hash();

        var issuedAt = _faker.Date.RecentOffset();
        _timeProvider.SetUtcNow(issuedAt);
        var tokenCreation = _timeProvider.GetUtcNow();
        var tokenExpiration = tokenCreation.AddDays(7);
        var refreshToken = FakeRefreshToken(tokenExpiration, tokenCreation);

        _refreshTokenFactory.Hash(command.RefreshToken).Returns(hash);
        _repository.GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>()).Returns(refreshToken);
        _timeProvider.Advance(TimeSpan.FromDays(8));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidRefreshTokenException>();
        _refreshTokenFactory.Received(1).Hash(command.RefreshToken);
        await _repository.Received(1).GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTokenIsRevoked_ShouldThrowInvalidRefreshToken()
    {
        // Arrange
        var command = FakeCommand();
        var hash = _faker.Random.Hash();

        var issuedAt = _faker.Date.RecentOffset();
        _timeProvider.SetUtcNow(issuedAt);
        var tokenCreation = _timeProvider.GetUtcNow();
        var tokenExpiration = tokenCreation.AddDays(7);
        var refreshToken = FakeRefreshToken(tokenExpiration, tokenCreation);
        refreshToken.Revoke(tokenCreation.AddDays(1));

        _refreshTokenFactory.Hash(command.RefreshToken).Returns(hash);
        _repository.GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>()).Returns(refreshToken);
        _timeProvider.Advance(TimeSpan.FromDays(5));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidRefreshTokenException>();
        _refreshTokenFactory.Received(1).Hash(command.RefreshToken);
        await _repository.Received(1).GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowInvalidRefreshToken()
    {
        // Arrange
        var command = FakeCommand();
        var hash = _faker.Random.Hash();

        var issuedAt = _faker.Date.RecentOffset();
        _timeProvider.SetUtcNow(issuedAt);
        var tokenCreation = _timeProvider.GetUtcNow();
        var tokenExpiration = tokenCreation.AddDays(7);
        var validToken = FakeRefreshToken(tokenExpiration, tokenCreation);

        _refreshTokenFactory.Hash(command.RefreshToken).Returns(hash);
        _repository.GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>()).Returns(validToken);
        _repository.GetByIdWithRolesAsync(validToken.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidRefreshTokenException>();
        await _repository.Received(1).GetByIdWithRolesAsync(validToken.UserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldThrowInvalidRefreshToken()
    {
        // Arrange
        var command = FakeCommand();
        var hash = _faker.Random.Hash();

        var issuedAt = _faker.Date.RecentOffset();
        _timeProvider.SetUtcNow(issuedAt);
        var tokenCreation = _timeProvider.GetUtcNow();
        var tokenExpiration = tokenCreation.AddDays(7);
        var validToken = FakeRefreshToken(tokenExpiration, tokenCreation);

        var user = FakeUser(false);

        _refreshTokenFactory.Hash(command.RefreshToken).Returns(hash);
        _repository.GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>()).Returns(validToken);
        _repository.GetByIdWithRolesAsync(validToken.UserId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidRefreshTokenException>();
        await _repository.Received(1).GetByIdWithRolesAsync(validToken.UserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStampSnapshotMismatches_ShouldThrowInvalidRefreshToken()
    {
        // Arrange
        var command = FakeCommand();
        var hash = _faker.Random.Hash();

        var user = FakeUser(true);

        var issuedAt = _faker.Date.RecentOffset();
        _timeProvider.SetUtcNow(issuedAt);
        var tokenCreation = _timeProvider.GetUtcNow();
        var tokenExpiration = tokenCreation.AddDays(7);
        var validToken = FakeRefreshToken(tokenExpiration, tokenCreation,
            string.Concat(user.SecurityStamp, "-", "Rotate"));

        _refreshTokenFactory.Hash(command.RefreshToken).Returns(hash);
        _repository.GetRefreshTokenByHashAsync(hash, Arg.Any<CancellationToken>()).Returns(validToken);
        _repository.GetByIdWithRolesAsync(validToken.UserId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidRefreshTokenException>();
    }

    private RefreshCommand FakeCommand() => new(_faker.Random.AlphaNumeric(43));

    private User FakeUser(bool isActive) =>
        new(_faker.Internet.Email(), _faker.Internet.Password(), _faker.Person.FullName, isActive);

    private RefreshToken FakeRefreshToken(DateTimeOffset expiresAt, DateTimeOffset createdAt,
        string stampSnapshot = "") =>
        new(_faker.Random.Int(), _faker.Random.Hash(), expiresAt, stampSnapshot, createdAt);
}
