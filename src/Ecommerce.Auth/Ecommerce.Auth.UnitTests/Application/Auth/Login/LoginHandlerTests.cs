using Ecommerce.Auth.Application.Auth.Login;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Application.Exceptions;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Auth.UnitTests.Application.Auth.Login;

public class LoginHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenFactory _refreshTokenFactory = Substitute.For<IRefreshTokenFactory>();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly Faker _faker = new();
    private readonly LoginHandler _handler;
    private const string DummyHash = "$2a$12$cFImGngfLrmQcxOsR1Np.Okd210KBzNKi/mxJU9NVmuaw8iKjf4Ve";

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(
            _repository, _passwordHasher, _jwtTokenGenerator, _refreshTokenFactory, _timeProvider);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowInvalidCredentialsAndCallVerifyWithDummyHash()
    {
        // Arrange
        var command = FakeCommand();
        _repository.GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidCredentialsException>();
        await _repository.Received(1).GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).Verify(command.Password, DummyHash);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldThrowInvalidCredentialsAndCallVerify()
    {
        // Arrange
        var command = FakeCommand();
        var user = FakeUser(false);

        _repository.GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash).Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidCredentialsException>();
        await _repository.Received(1).GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).Verify(command.Password, user.PasswordHash);
    }

    [Fact]
    public async Task Handle_WhenPasswordDoesNotMatch_ShouldThrowInvalidCredentialsAndCallVerify()
    {
        // Arrange
        var command = FakeCommand();
        var user = FakeUser(true);

        _repository.GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash).Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidCredentialsException>();
        await _repository.Received(1).GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).Verify(command.Password, user.PasswordHash);
    }

    [Fact]
    public async Task Handle_WhenLoginIsValid_ShouldSaveRefreshToken()
    {
        // Arrange
        var command = FakeCommand();
        var user = FakeUser(true);
        var now = _faker.Date.RecentOffset();
        var lifetime = TimeSpan.FromDays(_faker.Random.Int(1, 30));
        JwtAccessToken accessToken = new(_faker.Random.AlphaNumeric(32), _faker.Random.Int(1, 900));
        RefreshTokenPair refreshPair = new(_faker.Random.AlphaNumeric(43), _faker.Random.Hash());

        _timeProvider.SetUtcNow(now);
        _repository.GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash).Returns(true);
        _jwtTokenGenerator.Generate(user.Id, user.Email, Arg.Any<IEnumerable<RoleName>>()).Returns(accessToken);
        _refreshTokenFactory.Lifetime.Returns(lifetime);
        _refreshTokenFactory.Create().Returns(refreshPair);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).AddRefreshToken(Arg.Is<RefreshToken>(t =>
            t.UserId == user.Id &&
            t.TokenHash == refreshPair.Hash &&
            t.StampSnapshot == user.SecurityStamp &&
            t.ExpiresAt == now.Add(lifetime) &&
            t.CreatedAt == now));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private LoginCommand FakeCommand() => new(_faker.Internet.Email(), _faker.Internet.Password());

    private User FakeUser(bool isActive) =>
        new(_faker.Internet.Email(), _faker.Internet.Password(), _faker.Person.FullName, isActive);
}
