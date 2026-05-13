using Ecommerce.Auth.Application.Users.CreateUser;
using Ecommerce.Auth.Application.Users.Security;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Auth.UnitTests.Application.Users.CreateUser;

public class CreateUserHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _handler = new CreateUserHandler(_repository, _passwordHasher);
    }

    [Fact]
    public async Task Handle_WhenEmailIsUnique_ShouldHashPassword()
    {
        // Arrange
        var command = new CreateUserCommand("user@example.com", "Str0ngPass",
            "Jane", "Doe");
        _repository.CheckEmailExistsAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasher.Received(1).Hash(command.Password);
    }

    [Fact]
    public async Task Handle_WhenEmailIsUnique_ShouldAddUserAndSaveChanges()
    {
        // Arrange
        var command = new CreateUserCommand("user@example.com", "Str0ngPass",
            "Jane", "Doe");
        _repository.CheckEmailExistsAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Add(Arg.Any<User>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var command = new CreateUserCommand("user@example.com", "Str0ngPass",
            "Jane", "Doe");
        _repository.CheckEmailExistsAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BusinessRuleValidationException>();
    }
}
