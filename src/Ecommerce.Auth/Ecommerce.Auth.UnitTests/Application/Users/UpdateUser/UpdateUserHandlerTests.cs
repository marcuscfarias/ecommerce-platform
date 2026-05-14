using Ecommerce.Auth.Application.Users.UpdateUser;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Auth.UnitTests.Application.Users.UpdateUser;

public class UpdateUserHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly Faker _faker = new();
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _handler = new UpdateUserHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        User? user = null;
        var command = new UpdateUserCommand(1, _faker.Name.FirstName(), _faker.Name.LastName(), true);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldUpdateProfileAndSave()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), "hash", _faker.Name.FirstName(), _faker.Name.LastName());
        var command = new UpdateUserCommand(1, _faker.Name.FirstName(), _faker.Name.LastName(), false);
        _repository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.FirstName.ShouldBe(command.FirstName);
        user.LastName.ShouldBe(command.LastName);
        user.IsActive.ShouldBeFalse();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
