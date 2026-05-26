using Ecommerce.Auth.Application.Auth.Login;
using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Application.Exceptions;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;

namespace Ecommerce.Auth.UnitTests.Application.Auth.Login;

public class LoginHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly Faker _faker = new();
    private readonly LoginHandler _handler;
    private const string DummyHash = "$2a$12$cFImGngfLrmQcxOsR1Np.Okd210KBzNKi/mxJU9NVmuaw8iKjf4Ve";

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(_repository, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowInvalidCredentialsAndCallVerifyWithDummyHash()
    {
        // Arrange
        LoginCommand command = new(_faker.Internet.Email(), _faker.Internet.Password());
        _repository.GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

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
        LoginCommand command = new(_faker.Internet.Email(), _faker.Internet.Password());
        User user = new(command.Email, DummyHash, _faker.Person.FullName,
            isActive: false);

        _repository.GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash)
            .Returns(true);

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
        LoginCommand command = new(_faker.Internet.Email(), _faker.Internet.Password());
        User user = new(command.Email, DummyHash, _faker.Person.FullName,
            isActive: true);

        _repository.GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash)
            .Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidCredentialsException>();
        await _repository.Received(1).GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).Verify(command.Password, user.PasswordHash);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValidAndUserIsActive_ShouldReturnAccessToken()
    {
        // Arrange
        LoginCommand command = new(_faker.Internet.Email(), _faker.Internet.Password());
        User user = new(command.Email, DummyHash, _faker.Person.FullName,
            isActive: true);
        JwtAccessToken issuedToken = new(_faker.Random.AlphaNumeric(32), 900);

        _repository.GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash)
            .Returns(true);
        _jwtTokenGenerator.Generate(user.Id, user.Email, Arg.Any<IEnumerable<RoleName>>())
            .Returns(issuedToken);

        // Act
        LoginResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.ShouldBe(issuedToken.Token);
        result.TokenType.ShouldBe("Bearer");
        result.ExpiresInSeconds.ShouldBe(issuedToken.ExpiresInSeconds);
        await _repository.Received(1).GetByEmailWithRolesAsync(command.Email, Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).Verify(command.Password, user.PasswordHash);
        _jwtTokenGenerator.Received(1).Generate(user.Id, user.Email, Arg.Any<IEnumerable<RoleName>>());
    }
}
