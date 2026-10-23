using System.Globalization;
using Ecommerce.Auth.Application;
using Ecommerce.Auth.Application.Auth.GetMe;
using Ecommerce.Customers.Application.Profiles;
using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Customers.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Application.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Customers.UnitTests.Application.Profiles;

public class CurrentCustomerProfileTests
{
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IAuthModule _authModule = Substitute.For<IAuthModule>();
    private readonly FakeLogger<CurrentCustomerProfile> _logger = new();
    private readonly Faker _faker = new();
    private readonly List<Customer> _added = [];
    private readonly int _userId;
    private readonly CurrentCustomerProfile _profile;

    public CurrentCustomerProfileTests()
    {
        _userId = _faker.Random.Int(1);
        _userContext.UserId.Returns(_userId);
        _repository.Add(Arg.Do<Customer>(_added.Add));

        _profile = new CurrentCustomerProfile(_repository, _userContext, _authModule, _logger);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenProfileExists_ShouldReturnItWithoutCreatingAnother()
    {
        // Arrange
        var existing = new Customer(_userId, _faker.Name.FullName());
        existing.UpdateProfile(existing.DisplayName, $"+55{_faker.Random.ReplaceNumbers("###########")}");
        _repository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        var result = await _profile.GetOrCreateAsync(CancellationToken.None);

        // Assert
        result.ShouldBeSameAs(existing);
        _added.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenProfileIsMissing_ShouldAddCustomerWithAuthNameAndSaveOnce()
    {
        // Arrange
        var user = ArrangeAuthUser();

        // Act
        var result = await _profile.GetOrCreateAsync(CancellationToken.None);

        // Assert
        var added = _added.ShouldHaveSingleItem();
        added.UserId.ShouldBe(_userId);
        added.DisplayName.ShouldBe(user.Name);
        added.Phone.ShouldBeNull();
        result.ShouldBeSameAs(added);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenProfileIsMissing_ShouldLogProfileCreatedWithIdsAndNoName()
    {
        // Arrange
        var user = ArrangeAuthUser();

        // Act
        await _profile.GetOrCreateAsync(CancellationToken.None);

        // Assert
        var added = _added.ShouldHaveSingleItem();
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.Id.Id.ShouldBe(1);
        var state = record.StructuredState.ShouldNotBeNull();
        state.Select(e => e.Key).ShouldBe(["CustomerId", "UserId", "{OriginalFormat}"], ignoreOrder: true);
        state.ShouldContain(new KeyValuePair<string, string?>("CustomerId", added.Id.ToString(CultureInfo.InvariantCulture)));
        state.ShouldContain(new KeyValuePair<string, string?>("UserId", _userId.ToString(CultureInfo.InvariantCulture)));
        record.Message.ShouldNotContain(user.Name);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenAuthUserDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        _authModule.ExecuteQueryAsync(Arg.Any<GetMeQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<GetMeResult>(new ResourceNotFoundException("User", _userId)));

        // Act
        var act = () => _profile.GetOrCreateAsync(CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    private GetMeResult ArrangeAuthUser()
    {
        var user = new GetMeResult(_userId, _faker.Internet.Email(), _faker.Name.FullName(), [_faker.Random.Word()]);
        _authModule.ExecuteQueryAsync(Arg.Any<GetMeQuery>(), Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }
}
