using Ecommerce.Kernel.Application.Exceptions;
using Ecommerce.Kernel.Application.Security;
using Ecommerce.Orders.Application.Carts;
using Ecommerce.Orders.Application.Carts.RemoveCartItem;
using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Domain.Repositories;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Orders.UnitTests.Application.Carts.RemoveCartItem;

public class RemoveCartItemHandlerTests
{
    private readonly ICartRepository _repository = Substitute.For<ICartRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly Faker _faker = new();
    private readonly int _userId;
    private readonly RemoveCartItemHandler _handler;

    public RemoveCartItemHandlerTests()
    {
        _userId = _faker.Random.Int(1);
        _userContext.UserId.Returns(_userId);

        var currentCart = new CurrentCart(_repository, _userContext, new FakeLogger<CurrentCart>());
        _handler = new RemoveCartItemHandler(currentCart);
    }

    [Fact]
    public async Task Handle_WhenCustomerHasNoCart_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        var command = new RemoveCartItemCommand(_faker.Random.Int(1));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductIsNotInCart_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        ArrangeCart(_faker.Random.Int(1, 1000));
        var command = new RemoveCartItemCommand(_faker.Random.Int(1001, 2000));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductIsInCart_ShouldRemoveOnlyThatLineAndSaveOnce()
    {
        // Arrange
        var removedProductId = _faker.Random.Int(1, 1000);
        var keptProductId = _faker.Random.Int(1001, 2000);
        var cart = ArrangeCart(removedProductId, keptProductId);

        // Act
        await _handler.Handle(new RemoveCartItemCommand(removedProductId), CancellationToken.None);

        // Assert
        cart.Items.ShouldHaveSingleItem().ProductId.ShouldBe(keptProductId);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private Cart ArrangeCart(params int[] productIds)
    {
        var cart = new Cart(_userId);
        foreach (var productId in productIds)
            cart.AddItem(productId, _faker.Random.Int(1, 10));

        _repository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(cart);
        return cart;
    }
}
