using Ecommerce.Auth.Application;
using Ecommerce.Auth.Application.Auth.GetMe;
using Ecommerce.Customers.Application.Profiles;
using Ecommerce.Customers.Application.Profiles.GetMyProfile;
using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Customers.Domain.Repositories;
using Ecommerce.Kernel.Application.Security;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Customers.UnitTests.Application.Profiles.GetMyProfile;

public class GetMyProfileHandlerTests
{
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IAuthModule _authModule = Substitute.For<IAuthModule>();
    private readonly Faker _faker = new();
    private readonly int _userId;
    private readonly GetMyProfileHandler _handler;

    public GetMyProfileHandlerTests()
    {
        _userId = _faker.Random.Int(1);
        _userContext.UserId.Returns(_userId);

        var currentProfile = new CurrentCustomerProfile(
            _repository, _userContext, _authModule, new FakeLogger<CurrentCustomerProfile>());

        _handler = new GetMyProfileHandler(currentProfile);
    }

    [Fact]
    public async Task Handle_WhenProfileExists_ShouldReturnStoredDisplayNameAndPhone()
    {
        // Arrange
        var existing = new Customer(_userId, _faker.Name.FullName());
        existing.UpdateProfile(_faker.Name.FullName(), $"+55{_faker.Random.ReplaceNumbers("###########")}");
        _repository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        var result = await _handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        // Assert
        result.ShouldBe(new GetMyProfileResult(existing.DisplayName, existing.Phone));
    }

    [Fact]
    public async Task Handle_WhenProfileIsMissing_ShouldReturnAuthNameAndNullPhone()
    {
        // Arrange
        var user = new GetMeResult(_userId, _faker.Internet.Email(), _faker.Name.FullName(), [_faker.Random.Word()]);
        _authModule.ExecuteQueryAsync(Arg.Any<GetMeQuery>(), Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        // Assert
        result.ShouldBe(new GetMyProfileResult(user.Name, null));
    }
}
