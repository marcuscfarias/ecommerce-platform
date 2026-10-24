using Ecommerce.Catalog.Application;
using Ecommerce.Catalog.Application.Products.GetProductAvailability;
using Ecommerce.Customers.Application;
using Ecommerce.Customers.Application.Profiles.GetCurrentCustomerId;
using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Domain.Exceptions;
using Ecommerce.Orders.Application.Carts;
using Ecommerce.Orders.Application.Carts.ChangeCartItemQuantity;
using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Domain.Repositories;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Orders.UnitTests.Application.Carts.ChangeCartItemQuantity;

public class ChangeCartItemQuantityHandlerTests
{
    private readonly ICartRepository _repository = Substitute.For<ICartRepository>();
    private readonly ICustomerModule _customerModule = Substitute.For<ICustomerModule>();
    private readonly ICatalogModule _catalogModule = Substitute.For<ICatalogModule>();
    private readonly Faker _faker = new();
    private readonly int _customerId;
    private readonly ChangeCartItemQuantityHandler _handler;

    public ChangeCartItemQuantityHandlerTests()
    {
        _customerId = _faker.Random.Int(1);
        _customerModule.ExecuteQueryAsync(Arg.Any<GetCurrentCustomerIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(_customerId);

        var currentCart = new CurrentCart(_repository, _customerModule, new FakeLogger<CurrentCart>());
        _handler = new ChangeCartItemQuantityHandler(currentCart, new CatalogProducts(_catalogModule));
    }

    [Fact]
    public async Task Handle_WhenCustomerHasNoCart_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        var command = new ChangeCartItemQuantityCommand(_faker.Random.Int(1), 1);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductIsNotInCart_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        ArrangeCart((_faker.Random.Int(1, 1000), 1));
        var command = new ChangeCartItemQuantityCommand(_faker.Random.Int(1001, 2000), 1);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductIsInactiveAndOutOfStock_ShouldThrowNoLongerAvailableMessage()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 0, isActive: false);
        ArrangeCart((product.Id, 1));
        ArrangeCatalog(product);
        var command = new ChangeCartItemQuantityCommand(product.Id, 2);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<BusinessRuleValidationException>();
        exception.Message.ShouldBe("Product is no longer available.");
    }

    [Fact]
    public async Task Handle_WhenProductIsMissingFromCatalog_ShouldThrowNoLongerAvailableMessage()
    {
        // Arrange
        var productId = _faker.Random.Int(1);
        ArrangeCart((productId, 1));
        ArrangeCatalog();
        var command = new ChangeCartItemQuantityCommand(productId, 2);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<BusinessRuleValidationException>();
        exception.Message.ShouldBe("Product is no longer available.");
    }

    [Fact]
    public async Task Handle_WhenStockIsZero_ShouldThrowOutOfStockMessage()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 0, isActive: true);
        ArrangeCart((product.Id, 1));
        ArrangeCatalog(product);
        var command = new ChangeCartItemQuantityCommand(product.Id, 2);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<BusinessRuleValidationException>();
        exception.Message.ShouldBe("Product is out of stock.");
    }

    [Fact]
    public async Task Handle_WhenQuantityExceedsStock_ShouldThrowOnlyAvailableMessage()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 3, isActive: true);
        ArrangeCart((product.Id, 1));
        ArrangeCatalog(product);
        var command = new ChangeCartItemQuantityCommand(product.Id, 4);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<BusinessRuleValidationException>();
        exception.Message.ShouldBe("Only 3 available.");
    }

    [Fact]
    public async Task Handle_WhenDecreaseStillExceedsStock_ShouldThrowOnlyAvailableMessage()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 3, isActive: true);
        ArrangeCart((product.Id, 5));
        ArrangeCatalog(product);
        var command = new ChangeCartItemQuantityCommand(product.Id, 4);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<BusinessRuleValidationException>();
        exception.Message.ShouldBe("Only 3 available.");
    }

    [Fact]
    public async Task Handle_WhenStockCoversQuantity_ShouldReplaceTheQuantityAndSaveOnce()
    {
        // Arrange
        var product = NewProduct(stockQuantity: 5, isActive: true);
        var cart = ArrangeCart((product.Id, 2));
        ArrangeCatalog(product);
        var command = new ChangeCartItemQuantityCommand(product.Id, 4);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        cart.Items.ShouldHaveSingleItem().Quantity.ShouldBe(4);
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

    private void ArrangeCatalog(params ProductAvailability[] products) =>
        _catalogModule.ExecuteQueryAsync(Arg.Any<GetProductAvailabilityQuery>(), Arg.Any<CancellationToken>())
            .Returns(new GetProductAvailabilityResult(products));

    private ProductAvailability NewProduct(int stockQuantity, bool isActive) =>
        new(_faker.Random.Int(1),
            _faker.Commerce.ProductName(),
            _faker.Random.Decimal(1, 1000),
            stockQuantity,
            isActive,
            _faker.Random.Bool());
}
