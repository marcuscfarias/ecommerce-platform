using Ecommerce.Auth.Application.Users.SetUserStatus;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.Exceptions;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Auth.UnitTests.Application.Users.SetUserStatus;

public class SetUserStatusHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly Faker _faker = new();
    private readonly SetUserStatusHandler _handler;

    public SetUserStatusHandlerTests()
    {
        _handler = new SetUserStatusHandler(_repository, _timeProvider);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        User? user = null;
        var command = new SetUserStatusCommand(1, IsActive: false);
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
        var command = new SetUserStatusCommand(1, IsActive: false);
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BusinessRuleValidationException>();
    }

    [Fact]
    public async Task Handle_WhenDeactivatingActiveUser_ShouldDeactivateRevokeTokensAndSave()
    {
        // Arrange
        var now = _faker.Date.RecentOffset();
        _timeProvider.SetUtcNow(now);
        var user = new User(_faker.Internet.Email(), "hash", _faker.Name.FullName(), isActive: true);
        var token = new RefreshToken(user.Id, _faker.Random.Hash(), now.AddDays(7), user.SecurityStamp, now);
        var command = new SetUserStatusCommand(1, IsActive: false);
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);
        _repository.GetActiveRefreshTokensForUserAsync(user.Id, now, Arg.Any<CancellationToken>()).Returns([token]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.IsActive.ShouldBeFalse();
        token.RevokedAt.ShouldBe(now);
        _repository.Received(1).Update(user);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReactivatingInactiveUser_ShouldReactivateAndSave()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), "hash", _faker.Name.FullName(), isActive: false);
        var command = new SetUserStatusCommand(1, IsActive: true);
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.IsActive.ShouldBeTrue();
        _repository.Received(1).Update(user);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDeactivatingAlreadyInactiveUser_ShouldNotRotateStamp()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), "hash", _faker.Name.FullName(), isActive: false);
        var stampBeforeHandle = user.SecurityStamp;
        var command = new SetUserStatusCommand(1, IsActive: false);
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.IsActive.ShouldBeFalse();
        user.SecurityStamp.ShouldBe(stampBeforeHandle);
    }
}
