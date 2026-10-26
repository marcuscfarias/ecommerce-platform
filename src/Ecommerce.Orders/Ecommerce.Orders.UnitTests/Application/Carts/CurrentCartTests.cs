using System.Globalization;
using Ecommerce.Kernel.Application.Security;
using Ecommerce.Kernel.Domain.Entities;
using Ecommerce.Orders.Application.Carts;
using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Orders.UnitTests.Application.Carts;

public class CurrentCartTests
{
    private readonly ICartRepository _repository = Substitute.For<ICartRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly FakeLogger<CurrentCart> _logger = new();
    private readonly Faker _faker = new();
    private readonly List<Cart> _added = [];
    private readonly int _userId;
    private readonly CurrentCart _currentCart;

    public CurrentCartTests()
    {
        _userId = _faker.Random.Int(1);
        _userContext.UserId.Returns(_userId);
        _repository.Add(Arg.Do<Cart>(_added.Add));

        _currentCart = new CurrentCart(_repository, _userContext, _logger);
    }

    [Fact]
    public async Task FindAsync_WhenCustomerHasCart_ShouldReturnIt()
    {
        // Arrange
        var existing = ArrangeExistingCart();

        // Act
        var result = await _currentCart.FindAsync(CancellationToken.None);

        // Assert
        result.ShouldBeSameAs(existing);
    }

    [Fact]
    public async Task FindAsync_WhenCustomerHasNoCart_ShouldReturnNullAndAddNoCart()
    {
        // Act
        var result = await _currentCart.FindAsync(CancellationToken.None);

        // Assert
        result.ShouldBeNull();
        _added.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenCustomerHasCart_ShouldReturnItWithoutAddingAnother()
    {
        // Arrange
        var existing = ArrangeExistingCart();

        // Act
        var result = await _currentCart.GetOrCreateAsync(CancellationToken.None);

        // Assert
        result.ShouldBeSameAs(existing);
        _added.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenCustomerHasNoCart_ShouldAddAnEmptyCartForTheCurrentCustomer()
    {
        // Act
        var result = await _currentCart.GetOrCreateAsync(CancellationToken.None);

        // Assert
        var added = _added.ShouldHaveSingleItem();
        added.UserId.ShouldBe(_userId);
        added.Items.ShouldBeEmpty();
        result.ShouldBeSameAs(added);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCartWasCreated_ShouldSaveAndLogCartCreatedWithIdsOnly()
    {
        // Arrange
        var savedId = _faker.Random.Int(1);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            AssignId(_added.Single(), savedId);
            return 1;
        });
        await _currentCart.GetOrCreateAsync(CancellationToken.None);

        // Act
        await _currentCart.SaveChangesAsync(CancellationToken.None);

        // Assert
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        var record = _logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.Id.Id.ShouldBe(1);
        var state = record.StructuredState.ShouldNotBeNull();
        state.Select(e => e.Key).ShouldBe(["CartId", "UserId", "{OriginalFormat}"], ignoreOrder: true);
        state.ShouldContain(new KeyValuePair<string, string?>("CartId", savedId.ToString(CultureInfo.InvariantCulture)));
        state.ShouldContain(new KeyValuePair<string, string?>("UserId", _userId.ToString(CultureInfo.InvariantCulture)));
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCartAlreadyExisted_ShouldSaveWithoutLogging()
    {
        // Arrange
        ArrangeExistingCart();
        await _currentCart.GetOrCreateAsync(CancellationToken.None);

        // Act
        await _currentCart.SaveChangesAsync(CancellationToken.None);

        // Assert
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _logger.Collector.Count.ShouldBe(0);
    }

    private Cart ArrangeExistingCart()
    {
        var existing = new Cart(_userId);
        existing.AddItem(_faker.Random.Int(1), _faker.Random.Int(1, 10));
        _repository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(existing);
        return existing;
    }

    private static void AssignId(Cart cart, int id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(cart, id);
}
