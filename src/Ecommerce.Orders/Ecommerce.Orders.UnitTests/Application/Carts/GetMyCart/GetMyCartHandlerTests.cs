using Ecommerce.Kernel.Application.Security;
using Ecommerce.Kernel.Domain.Entities;
using Ecommerce.Orders.Application.Carts;
using Ecommerce.Orders.Application.Carts.GetMyCart;
using Ecommerce.Orders.Domain.Catalog;
using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Domain.Repositories;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Orders.UnitTests.Application.Carts.GetMyCart;

public class GetMyCartHandlerTests
{
    private readonly ICartRepository _repository = Substitute.For<ICartRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly ICatalogProducts _catalogProducts = Substitute.For<ICatalogProducts>();
    private readonly Faker _faker = new();
    private readonly List<Cart> _added = [];
    private int _lastProductId;
    private readonly int _userId;
    private readonly GetMyCartHandler _handler;

    public GetMyCartHandlerTests()
    {
        _userId = _faker.Random.Int(1);
        _userContext.UserId.Returns(_userId);
        _repository.Add(Arg.Do<Cart>(_added.Add));

        var currentCart = new CurrentCart(_repository, _userContext, new FakeLogger<CurrentCart>());
        _handler = new GetMyCartHandler(currentCart, _catalogProducts);
    }

    [Fact]
    public async Task Handle_WhenCustomerHasNoCart_ShouldReturnEmptyCartWithZeroTotalsAndAddNoCart()
    {
        // Act
        var result = await _handler.Handle(new GetMyCartQuery(), CancellationToken.None);

        // Assert
        result.Items.ShouldBeEmpty();
        result.TotalQuantity.ShouldBe(0);
        result.Total.ShouldBe(0m);
        _added.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCartHasNoLines_ShouldReturnEmptyCartWithZeroTotals()
    {
        // Arrange
        ArrangeCart();

        // Act
        var result = await _handler.Handle(new GetMyCartQuery(), CancellationToken.None);

        // Assert
        result.Items.ShouldBeEmpty();
        result.TotalQuantity.ShouldBe(0);
        result.Total.ShouldBe(0m);
    }

    [Fact]
    public async Task Handle_WhenLineIsAvailable_ShouldReturnCurrentCatalogNamePriceAndImageFlag()
    {
        // Arrange
        var product = NewProduct(price: 12.50m, stockQuantity: 10, isActive: true, hasImage: true);
        ArrangeCart((product.Id, 3));
        ArrangeCatalog(product);

        // Act
        var result = await _handler.Handle(new GetMyCartQuery(), CancellationToken.None);

        // Assert
        result.Items.ShouldHaveSingleItem().ShouldBe(new GetMyCartItemResult(
            product.Id,
            product.Name,
            UnitPrice: 12.50m,
            Quantity: 3,
            Subtotal: 37.50m,
            HasImage: true,
            CartItemStatus.Available,
            AvailableQuantity: null));
    }

    [Fact]
    public async Task Handle_WhenLinesHaveEveryStatus_ShouldDeriveEachStatusSubtotalAndAvailableQuantity()
    {
        // Arrange
        var (available, outOfStock, insufficient, unavailable) = ArrangeFourStatusCart();

        // Act
        var result = await _handler.Handle(new GetMyCartQuery(), CancellationToken.None);

        // Assert
        result.Items.Select(i => (i.ProductId, i.Status, i.Subtotal, i.AvailableQuantity)).ShouldBe([
            (available, CartItemStatus.Available, 20.00m, (int?)null),
            (outOfStock, CartItemStatus.OutOfStock, 20.00m, null),
            (insufficient, CartItemStatus.InsufficientStock, 90.00m, 1),
            (unavailable, CartItemStatus.Unavailable, 40.00m, null),
        ]);
    }

    [Fact]
    public async Task Handle_WhenLinesHaveEveryStatus_ShouldSumQuantityOfAllLinesAndTotalOfAvailableLinesOnly()
    {
        // Arrange
        ArrangeFourStatusCart();

        // Act
        var result = await _handler.Handle(new GetMyCartQuery(), CancellationToken.None);

        // Assert
        result.TotalQuantity.ShouldBe(7);
        result.Total.ShouldBe(20.00m);
    }

    [Fact]
    public async Task Handle_WhenProductIsMissingFromCatalog_ShouldReturnLineAsUnavailable()
    {
        // Arrange
        var productId = _faker.Random.Int(1);
        ArrangeCart((productId, 2));
        ArrangeCatalog();

        // Act
        var result = await _handler.Handle(new GetMyCartQuery(), CancellationToken.None);

        // Assert
        var item = result.Items.ShouldHaveSingleItem();
        item.ProductId.ShouldBe(productId);
        item.Quantity.ShouldBe(2);
        item.Name.ShouldBeEmpty();
        item.UnitPrice.ShouldBe(0m);
        item.Subtotal.ShouldBe(0m);
        item.Status.ShouldBe(CartItemStatus.Unavailable);
        item.AvailableQuantity.ShouldBeNull();
        result.Total.ShouldBe(0m);
    }

    [Fact]
    public async Task Handle_WhenLineIdsDifferFromInsertionOrder_ShouldOrderItemsByLineId()
    {
        // Arrange
        var first = NewProduct(price: 10.00m, stockQuantity: 5, isActive: true);
        var second = NewProduct(price: 10.00m, stockQuantity: 5, isActive: true);
        var cart = ArrangeCart((first.Id, 1), (second.Id, 1));
        AssignId(cart.FindItem(first.Id)!, 2);
        AssignId(cart.FindItem(second.Id)!, 1);
        ArrangeCatalog(first, second);

        // Act
        var result = await _handler.Handle(new GetMyCartQuery(), CancellationToken.None);

        // Assert
        result.Items.Select(i => i.ProductId).ShouldBe([second.Id, first.Id]);
    }

    private (int Available, int OutOfStock, int Insufficient, int Unavailable) ArrangeFourStatusCart()
    {
        var available = NewProduct(price: 10.00m, stockQuantity: 5, isActive: true);
        var outOfStock = NewProduct(price: 20.00m, stockQuantity: 0, isActive: true);
        var insufficient = NewProduct(price: 30.00m, stockQuantity: 1, isActive: true);
        var unavailable = NewProduct(price: 40.00m, stockQuantity: 5, isActive: false);
        ArrangeCart((available.Id, 2), (outOfStock.Id, 1), (insufficient.Id, 3), (unavailable.Id, 1));
        ArrangeCatalog(available, outOfStock, insufficient, unavailable);

        return (available.Id, outOfStock.Id, insufficient.Id, unavailable.Id);
    }

    private Cart ArrangeCart(params (int ProductId, int Quantity)[] lines)
    {
        var cart = new Cart(_userId);
        foreach (var (productId, quantity) in lines)
            cart.AddItem(productId, quantity);

        _repository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(cart);
        return cart;
    }

    private void ArrangeCatalog(params CatalogProduct[] products) =>
        _catalogProducts.GetAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(products.ToDictionary(p => p.Id));

    private CatalogProduct NewProduct(decimal price, int stockQuantity, bool isActive, bool hasImage = false) =>
        new(++_lastProductId,
            _faker.Commerce.ProductName(),
            price,
            stockQuantity,
            isActive,
            hasImage);

    private static void AssignId(Entity entity, int id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id);
}
