using Ecommerce.Auth.Application.Users.DeleteUser;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;

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
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldRemoveAndSave()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), "hash", _faker.Name.FirstName(), _faker.Name.LastName());
        var command = new DeleteUserCommand(1);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Remove(user);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
