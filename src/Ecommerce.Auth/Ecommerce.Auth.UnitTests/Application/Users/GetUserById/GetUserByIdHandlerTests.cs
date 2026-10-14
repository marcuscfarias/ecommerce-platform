using Ecommerce.Auth.Application.Users.GetUserById;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;

namespace Ecommerce.Auth.UnitTests.Application.Users.GetUserById;

public class GetUserByIdHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly Faker _faker = new();
    private readonly GetUserByIdHandler _handler;

    public GetUserByIdHandlerTests()
    {
        _handler = new GetUserByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        User? user = null;
        var query = new GetUserByIdQuery(1);
        _repository.GetByIdWithRolesAsync(query.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsCustomer_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        var user = new User(_faker.Internet.Email(), _faker.Random.Hash(), _faker.Name.FullName());
        user.AssignRole(new Role(nameof(RoleName.Customer)));
        var query = new GetUserByIdQuery(_faker.Random.Int(1));
        _repository.GetByIdWithRolesAsync(query.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnMappedResult()
    {
        // Arrange
        var user = new User("jane@example.com", "hash", "Jane Doe");
        user.AssignRole(new Role(nameof(RoleName.Manager)));
        var query = new GetUserByIdQuery(1);
        _repository.GetByIdWithRolesAsync(query.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Id.ShouldBe(user.Id);
        result.Email.ShouldBe(user.Email);
        result.Name.ShouldBe(user.Name);
        result.IsActive.ShouldBe(user.IsActive);
        result.Roles.ShouldBe(["Manager"]);
    }
}
