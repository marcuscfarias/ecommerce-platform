using System.Globalization;
using Ecommerce.Customers.Application.Profiles;
using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Customers.Domain.Repositories;
using Ecommerce.Kernel.Application.Security;
using Ecommerce.Kernel.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Customers.UnitTests.Application.Profiles;

public class CurrentCustomerProfileTests
{
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
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

        _profile = new CurrentCustomerProfile(_repository, _userContext, _logger);
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
    public async Task GetOrCreateAsync_WhenProfileIsMissing_ShouldAddCustomerWithTokenNameAndSaveOnce()
    {
        // Arrange
        var name = ArrangeTokenName();

        // Act
        var result = await _profile.GetOrCreateAsync(CancellationToken.None);

        // Assert
        var added = _added.ShouldHaveSingleItem();
        added.UserId.ShouldBe(_userId);
        added.DisplayName.ShouldBe(name);
        added.Phone.ShouldBeNull();
        result.ShouldBeSameAs(added);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenProfileIsMissing_ShouldLogProfileCreatedWithIdsAndNoName()
    {
        // Arrange
        var name = ArrangeTokenName();
        var savedId = _faker.Random.Int(1);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            AssignId(_added.Single(), savedId);
            return 1;
        });

        // Act
        await _profile.GetOrCreateAsync(CancellationToken.None);

        // Assert
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.Id.Id.ShouldBe(1);
        var state = record.StructuredState.ShouldNotBeNull();
        state.Select(e => e.Key).ShouldBe(["CustomerId", "UserId", "{OriginalFormat}"], ignoreOrder: true);
        state.ShouldContain(new KeyValuePair<string, string?>("CustomerId", savedId.ToString(CultureInfo.InvariantCulture)));
        state.ShouldContain(new KeyValuePair<string, string?>("UserId", _userId.ToString(CultureInfo.InvariantCulture)));
        record.Message.ShouldNotContain(name);
    }

    private string ArrangeTokenName()
    {
        var name = _faker.Name.FullName();
        _userContext.Name.Returns(name);
        return name;
    }

    private static void AssignId(Customer customer, int id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(customer, id);
}
