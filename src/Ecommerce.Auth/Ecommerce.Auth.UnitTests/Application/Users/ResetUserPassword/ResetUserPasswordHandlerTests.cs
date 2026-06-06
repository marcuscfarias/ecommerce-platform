using Ecommerce.Auth.Application.Auth.Security;
using Ecommerce.Auth.Application.Users.ResetUserPassword;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.Exceptions;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Auth.UnitTests.Application.Users.ResetUserPassword;

public class ResetUserPasswordHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly Faker _faker = new();
    private readonly ResetUserPasswordHandler _handler;

    public ResetUserPasswordHandlerTests()
    {
        _handler = new ResetUserPasswordHandler(_repository, _passwordHasher, _timeProvider);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        User? user = null;
        var command = new ResetUserPasswordCommand(1, _faker.Internet.Password());
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
        var command = new ResetUserPasswordCommand(1, _faker.Internet.Password());
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<BusinessRuleValidationException>();
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_ShouldHashPasswordRotateStampRevokeTokensAndSave()
    {
        // Arrange
        var now = _faker.Date.RecentOffset();
        _timeProvider.SetUtcNow(now);
        var user = new User(_faker.Internet.Email(), "old-hash", _faker.Name.FullName());
        var originalStamp = user.SecurityStamp;
        var token = new RefreshToken(user.Id, _faker.Random.Hash(), now.AddDays(7), user.SecurityStamp, now);
        var newHash = _faker.Random.AlphaNumeric(60);
        var command = new ResetUserPasswordCommand(1, _faker.Internet.Password());
        _repository.GetByIdWithRolesAsync(command.Id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Hash(command.Password).Returns(newHash);
        _repository.GetActiveRefreshTokensForUserAsync(user.Id, now, Arg.Any<CancellationToken>()).Returns([token]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.PasswordHash.ShouldBe(newHash);
        user.SecurityStamp.ShouldNotBe(originalStamp);
        token.RevokedAt.ShouldBe(now);
        _repository.Received(1).Update(user);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
