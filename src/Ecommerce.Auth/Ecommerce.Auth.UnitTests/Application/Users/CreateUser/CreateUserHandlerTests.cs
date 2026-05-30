using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Application.Users.CreateUser;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Auth.UnitTests.Application.Users.CreateUser;

public class CreateUserHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly Faker _faker = new();
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _repository.GetRoleByNameAsync(RoleName.Manager, Arg.Any<CancellationToken>())
            .Returns(new Role(nameof(RoleName.Manager)));

        _handler = new CreateUserHandler(_repository, _passwordHasher);
    }

    [Fact]
    public async Task Handle_WhenEmailIsUnique_ShouldCreateUserWithHashedPassword()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), _faker.Internet.Password(), _faker.Name.FullName());
        _repository.CheckEmailExistsAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(command.Password).Returns("hashed-password");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasher.Received(1).Hash(command.Password);
        _repository.Received(1).Add(Arg.Is<User>(u => u.PasswordHash == "hashed-password"));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserCreated_ShouldAssignManagerRole()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), _faker.Internet.Password(), _faker.Name.FullName());
        _repository.CheckEmailExistsAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(command.Password).Returns(_faker.Random.AlphaNumeric(60));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Add(Arg.Is<User>(u =>
            u.Roles.Any(r => r.Name == nameof(RoleName.Manager))));
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var command = new CreateUserCommand(_faker.Internet.Email(), _faker.Internet.Password(), _faker.Name.FullName());
        _repository.CheckEmailExistsAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BusinessRuleValidationException>();
    }
}
