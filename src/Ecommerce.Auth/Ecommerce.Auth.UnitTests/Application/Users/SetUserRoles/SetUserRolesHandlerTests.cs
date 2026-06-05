using Ecommerce.Auth.Application.Users.SetUserRoles;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Auth.UnitTests.Application.Users.SetUserRoles;

public class SetUserRolesHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly Faker _faker = new();
    private readonly SetUserRolesHandler _handler;

    public SetUserRolesHandlerTests()
    {
        _handler = new SetUserRolesHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        User? user = null;
        var command = new SetUserRolesCommand(1, [RoleName.Owner]);
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenTargetIsAdmin_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), "hash", _faker.Name.FullName());
        user.AssignRole(new Role(nameof(RoleName.Admin)));
        var command = new SetUserRolesCommand(1, [RoleName.Owner]);
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BusinessRuleValidationException>();
    }

    [Fact]
    public async Task Handle_WhenRolesAreValid_ShouldReplaceUserRolesAndSave()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), "hash", _faker.Name.FullName());
        user.AssignRole(new Role(nameof(RoleName.Manager)));
        var ownerRole = new Role(nameof(RoleName.Owner));
        var command = new SetUserRolesCommand(1, [RoleName.Owner]);
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);
        _repository.GetRoleByNameAsync(RoleName.Owner, Arg.Any<CancellationToken>()).Returns(ownerRole);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.Roles.Count.ShouldBe(1);
        user.Roles.ShouldContain(ownerRole);
        _repository.Received(1).Update(user);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
