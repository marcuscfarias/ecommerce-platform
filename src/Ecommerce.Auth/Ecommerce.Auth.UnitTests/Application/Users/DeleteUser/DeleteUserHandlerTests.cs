using Ecommerce.Auth.Application.Users.DeleteUser;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Auth.UnitTests.Application.Users.DeleteUser;

public class DeleteUserHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly Faker _faker = new();
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _handler = new DeleteUserHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        User? user = null;
        var command = new DeleteUserCommand(1);
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ShouldDeactivateAndSave()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), "hash", _faker.Name.FullName());
        var command = new DeleteUserCommand(1);
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.IsActive.ShouldBeFalse();
        _repository.Received(1).Update(user);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), "hash", _faker.Name.FullName());
        user.AssignRole(new Role(nameof(RoleName.Admin)));
        var command = new DeleteUserCommand(1);
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BusinessRuleValidationException>();
    }
}
