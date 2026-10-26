using Ecommerce.Customers.Application;
using Ecommerce.Customers.Application.Profiles.GetCurrentCustomerId;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.Exceptions;
using Ecommerce.Orders.Application.Carts;
using Ecommerce.Orders.Application.Carts.AddCartItem;
using Ecommerce.Orders.Domain.Catalog;
using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Domain.Repositories;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Orders.UnitTests.Application.Carts.AddCartItem;

public class AddCartItemHandlerTests
{
    private readonly ICartRepository _repository = Substitute.For<ICartRepository>();
    private readonly ICustomerModule _customerModule = Substitute.For<ICustomerModule>();
    private readonly ICatalogProducts _catalogProducts = Substitute.For<ICatalogProducts>();
    private readonly Faker _faker = new();
    private readonly List<Cart> _added = [];
    private readonly int _customerId;
    private readonly AddCartItemHandler _handler;

    public AddCartItemHandlerTests()
    {
        _customerId = _faker.Random.Int(1);
        _customerModule.ExecuteQueryAsync(Arg.Any<GetCurrentCustomerIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(_customerId);
        _repository.Add(Arg.Do<Cart>(_added.Add));

        var currentCart = new CurrentCart(_repository, _customerModule, new FakeLogger<CurrentCart>());
        _handler = new AddCartItemHandler(currentCart, _catalogProducts);
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldThrowResourceNotFoundExceptionAndCreateNoCart()
    {
        // Arrange
        ArrangeCatalog();
        var command = new AddCartItemCommand(_faker.Random.Int(1), 1);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
        _added.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenProductIsInactive_ShouldThrowResourceNotFoundExceptionAndCreateNoCart()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 5, isActive: false);
        ArrangeCatalog(product);
        var command = new AddCartItemCommand(product.Id, 1);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
        _added.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenResultingQuantityExceedsMaximumAndStock_ShouldThrowMaximumMessage()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 2, isActive: true);
        ArrangeCatalog(product);
        ArrangeCart((product.Id, 8));
        var command = new AddCartItemCommand(product.Id, 3);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<BusinessRuleValidationException>();
        exception.Message.ShouldBe("Maximum of 10 units per product.");
    }

    [Fact]
    public async Task Handle_WhenResultingQuantityExceedsMaximumAndStockIsZero_ShouldThrowMaximumMessage()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 0, isActive: true);
        ArrangeCatalog(product);
        ArrangeCart((product.Id, 8));
        var command = new AddCartItemCommand(product.Id, 3);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<BusinessRuleValidationException>();
        exception.Message.ShouldBe("Maximum of 10 units per product.");
    }

    [Fact]
    public async Task Handle_WhenStockIsZero_ShouldThrowOutOfStockMessage()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 0, isActive: true);
        ArrangeCatalog(product);
        var command = new AddCartItemCommand(product.Id, 1);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<BusinessRuleValidationException>();
        exception.Message.ShouldBe("Product is out of stock.");
    }

    [Fact]
    public async Task Handle_WhenResultingQuantityExceedsStock_ShouldThrowOnlyAvailableMessage()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 3, isActive: true);
        ArrangeCatalog(product);
        ArrangeCart((product.Id, 2));
        var command = new AddCartItemCommand(product.Id, 2);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<BusinessRuleValidationException>();
        exception.Message.ShouldBe("Only 3 available.");
    }

    [Fact]
    public async Task Handle_WhenCustomerHasNoCart_ShouldCreateCartWithTheLineAndSaveOnce()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 5, isActive: true);
        ArrangeCatalog(product);
        var command = new AddCartItemCommand(product.Id, 2);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var cart = _added.ShouldHaveSingleItem();
        cart.CustomerId.ShouldBe(_customerId);
        var item = cart.Items.ShouldHaveSingleItem();
        item.ProductId.ShouldBe(product.Id);
        item.Quantity.ShouldBe(2);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductIsAlreadyInCart_ShouldSumIntoTheExistingLineAndSaveOnce()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 10, isActive: true);
        ArrangeCatalog(product);
        var cart = ArrangeCart((product.Id, 2));
        var command = new AddCartItemCommand(product.Id, 3);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        cart.Items.ShouldHaveSingleItem().Quantity.ShouldBe(5);
        _added.ShouldBeEmpty();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private Cart ArrangeCart(params (int ProductId, int Quantity)[] lines)
    {
        var cart = new Cart(_customerId);
        foreach (var (productId, quantity) in lines)
            cart.AddItem(productId, quantity);

        _repository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>()).Returns(cart);
        return cart;
    }

    private void ArrangeCatalog(params CatalogProduct[] products) =>
        _catalogProducts.GetAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(products.ToDictionary(p => p.Id));

    private CatalogProduct NewProduct(int stockQuantity, bool isActive) =>
        new(_faker.Random.Int(1),
            _faker.Commerce.ProductName(),
            _faker.Random.Decimal(1, 1000),
            stockQuantity,
            isActive,
            _faker.Random.Bool());
}
