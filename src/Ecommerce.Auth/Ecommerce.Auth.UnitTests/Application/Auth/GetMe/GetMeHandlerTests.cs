using Ecommerce.Auth.Application.Auth.GetMe;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Application.Security;

namespace Ecommerce.Auth.UnitTests.Application.Auth.GetMe;

public class GetMeHandlerTests
{
    private static readonly Faker Faker = new();

    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly GetMeHandler _handler;

    public GetMeHandlerTests()
    {
        _handler = new GetMeHandler(_repository, _userContext);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnMappedResult()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        var user = new User(Faker.Internet.Email(), "hash", Faker.Name.FullName());
        user.AssignRole(new Role("Administrator"));
        _userContext.UserId.Returns(userId);
        _repository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _handler.Handle(new GetMeQuery(), CancellationToken.None);

        // Assert
        result.Id.ShouldBe(user.Id);
        result.Email.ShouldBe(user.Email);
        result.Name.ShouldBe(user.Name);
        result.Roles.ShouldBe(["Administrator"]);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        var userId = Faker.Random.Int(1, int.MaxValue);
        _userContext.UserId.Returns(userId);
        _repository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var act = () => _handler.Handle(new GetMeQuery(), CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }
}
