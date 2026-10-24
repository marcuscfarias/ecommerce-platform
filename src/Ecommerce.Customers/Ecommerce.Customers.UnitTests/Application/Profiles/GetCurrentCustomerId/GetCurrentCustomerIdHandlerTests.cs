using Ecommerce.Auth.Application;
using Ecommerce.Auth.Application.Auth.GetMe;
using Ecommerce.Customers.Application.Profiles;
using Ecommerce.Customers.Application.Profiles.GetCurrentCustomerId;
using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Customers.Domain.Repositories;
using Ecommerce.Kernel.Application.Security;
using Ecommerce.Kernel.Domain.Entities;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Customers.UnitTests.Application.Profiles.GetCurrentCustomerId;

public class GetCurrentCustomerIdHandlerTests
{
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IAuthModule _authModule = Substitute.For<IAuthModule>();
    private readonly Faker _faker = new();
    private readonly List<Customer> _added = [];
    private readonly int _userId;
    private readonly GetCurrentCustomerIdHandler _handler;

    public GetCurrentCustomerIdHandlerTests()
    {
        _userId = _faker.Random.Int(1);
        _userContext.UserId.Returns(_userId);
        _repository.Add(Arg.Do<Customer>(_added.Add));

        var currentProfile = new CurrentCustomerProfile(
            _repository, _userContext, _authModule, new FakeLogger<CurrentCustomerProfile>());

        _handler = new GetCurrentCustomerIdHandler(currentProfile);
    }

    [Fact]
    public async Task Handle_WhenProfileExists_ShouldReturnItsId()
    {
        // Arrange
        var existing = new Customer(_userId, _faker.Name.FullName());
        var existingId = _faker.Random.Int(1);
        AssignId(existing, existingId);
        _repository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        var result = await _handler.Handle(new GetCurrentCustomerIdQuery(), CancellationToken.None);

        // Assert
        result.ShouldBe(existingId);
        _added.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenProfileIsMissing_ShouldCreateItAndReturnTheNewId()
    {
        // Arrange
        var user = new GetMeResult(_userId, _faker.Internet.Email(), _faker.Name.FullName(), [_faker.Random.Word()]);
        _authModule.ExecuteQueryAsync(Arg.Any<GetMeQuery>(), Arg.Any<CancellationToken>()).Returns(user);
        var savedId = _faker.Random.Int(1);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            AssignId(_added.Single(), savedId);
            return 1;
        });

        // Act
        var result = await _handler.Handle(new GetCurrentCustomerIdQuery(), CancellationToken.None);

        // Assert
        result.ShouldBe(savedId);
        _added.ShouldHaveSingleItem().UserId.ShouldBe(_userId);
    }

    private static void AssignId(Customer customer, int id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(customer, id);
}
