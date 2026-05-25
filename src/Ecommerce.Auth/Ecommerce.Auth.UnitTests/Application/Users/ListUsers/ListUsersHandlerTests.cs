using Ecommerce.Auth.Application.Users.ListUsers;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.Domain.Repositories;
using Ecommerce.Kernel.Domain.Models;

namespace Ecommerce.Auth.UnitTests.Application.Users.ListUsers;

public class ListUsersHandlerTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly Faker _faker = new();
    private readonly ListUsersHandler _handler;

    public ListUsersHandlerTests()
    {
        _handler = new ListUsersHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenUsersExist_ShouldReturnMappedResults()
    {
        // Arrange
        var users = new List<User>
        {
            new(_faker.Internet.Email(), "hash", _faker.Name.FullName()),
            new(_faker.Internet.Email(), "hash", _faker.Name.FullName())
        };
        var pagedResult = new PagedResult<User>(users, Page: 1, TotalCount: 2, TotalPages: 1);
        var query = new ListUsersQuery(1);
        _repository.GetAllAsync(query.PageNumber, Arg.Any<CancellationToken>()).Returns(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Count.ShouldBe(2);
        result.Page.ShouldBe(1);
        result.TotalCount.ShouldBe(2);
        result.TotalPages.ShouldBe(1);

        result.Data[0].Name.ShouldBe(users[0].Name);
        result.Data[0].IsActive.ShouldBeTrue();

        result.Data[1].Name.ShouldBe(users[1].Name);
        result.Data[1].IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenNoUsersExist_ShouldReturnEmptyResult()
    {
        // Arrange
        var pagedResult = new PagedResult<User>([], Page: 1, TotalCount: 0, TotalPages: 0);
        var query = new ListUsersQuery(1);
        _repository.GetAllAsync(query.PageNumber, Arg.Any<CancellationToken>()).Returns(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.ShouldBeEmpty();
        result.Page.ShouldBe(1);
        result.TotalCount.ShouldBe(0);
        result.TotalPages.ShouldBe(0);
    }
}
