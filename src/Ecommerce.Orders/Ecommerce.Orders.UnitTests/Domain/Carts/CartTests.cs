using Ecommerce.Kernel.Domain.Exceptions;
using Ecommerce.Orders.Domain.Entities;

namespace Ecommerce.Orders.UnitTests.Domain.Carts;

public class CartTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_WithUserId_ShouldSetItAndStartWithNoItems()
    {
        // Arrange
        var userId = _faker.Random.Int(1);

        // Act
        var cart = new Cart(userId);

        // Assert
        cart.UserId.ShouldBe(userId);
        cart.Items.ShouldBeEmpty();
    }

    [Fact]
    public void AddItem_WithNewProduct_ShouldAppendLineWithQuantity()
    {
        // Arrange
        var cart = new Cart(_faker.Random.Int(1));
        var productId = _faker.Random.Int(1);
        var quantity = _faker.Random.Int(1, 10);

        // Act
        cart.AddItem(productId, quantity);

        // Assert
        var item = cart.Items.ShouldHaveSingleItem();
        item.ProductId.ShouldBe(productId);
        item.Quantity.ShouldBe(quantity);
    }

    [Fact]
    public void AddItem_WithProductAlreadyInCart_ShouldSumIntoSingleLine()
    {
        // Arrange
        var cart = new Cart(_faker.Random.Int(1));
        var productId = _faker.Random.Int(1);
        var firstQuantity = _faker.Random.Int(1, 5);
        var secondQuantity = _faker.Random.Int(1, 5);
        cart.AddItem(productId, firstQuantity);

        // Act
        cart.AddItem(productId, secondQuantity);

        // Assert
        var item = cart.Items.ShouldHaveSingleItem();
        item.ProductId.ShouldBe(productId);
        item.Quantity.ShouldBe(firstQuantity + secondQuantity);
    }

    [Fact]
    public void AddItem_WithResultingQuantityAtMaximum_ShouldAcceptIt()
    {
        // Arrange
        var cart = new Cart(_faker.Random.Int(1));
        var productId = _faker.Random.Int(1);
        var firstQuantity = _faker.Random.Int(1, 9);
        cart.AddItem(productId, firstQuantity);

        // Act
        cart.AddItem(productId, 10 - firstQuantity);

        // Assert
        cart.Items.ShouldHaveSingleItem().Quantity.ShouldBe(10);
    }

    [Fact]
    public void AddItem_WithNewProductAboveMaximum_ShouldThrowAndAddNoLine()
    {
        // Arrange
        var cart = new Cart(_faker.Random.Int(1));

        // Act
        var act = () => cart.AddItem(_faker.Random.Int(1), 11);

        // Assert
        Should.Throw<BusinessRuleValidationException>(act).Message.ShouldBe("Maximum of 10 units per product.");
        cart.Items.ShouldBeEmpty();
    }

    [Fact]
    public void AddItem_WithResultingQuantityAboveMaximum_ShouldThrowAndKeepLineUnchanged()
    {
        // Arrange
        var cart = new Cart(_faker.Random.Int(1));
        var productId = _faker.Random.Int(1);
        var firstQuantity = _faker.Random.Int(1, 9);
        cart.AddItem(productId, firstQuantity);

        // Act
        var act = () => cart.AddItem(productId, 11 - firstQuantity);

        // Assert
        Should.Throw<BusinessRuleValidationException>(act).Message.ShouldBe("Maximum of 10 units per product.");
        cart.Items.ShouldHaveSingleItem().Quantity.ShouldBe(firstQuantity);
    }

    [Fact]
    public void ChangeItemQuantity_WithProductInCart_ShouldReplaceQuantity()
    {
        // Arrange
        var cart = new Cart(_faker.Random.Int(1));
        var productId = _faker.Random.Int(1);
        cart.AddItem(productId, 3);

        // Act
        cart.ChangeItemQuantity(productId, 7);

        // Assert
        cart.Items.ShouldHaveSingleItem().Quantity.ShouldBe(7);
    }

    [Fact]
    public void ChangeItemQuantity_WithQuantityAboveMaximum_ShouldThrowAndKeepLineUnchanged()
    {
        // Arrange
        var cart = new Cart(_faker.Random.Int(1));
        var productId = _faker.Random.Int(1);
        var quantity = _faker.Random.Int(1, 10);
        cart.AddItem(productId, quantity);

        // Act
        var act = () => cart.ChangeItemQuantity(productId, 11);

        // Assert
        Should.Throw<BusinessRuleValidationException>(act).Message.ShouldBe("Maximum of 10 units per product.");
        cart.Items.ShouldHaveSingleItem().Quantity.ShouldBe(quantity);
    }

    [Fact]
    public void RemoveItem_WithProductInCart_ShouldDeleteOnlyThatLine()
    {
        // Arrange
        var cart = new Cart(_faker.Random.Int(1));
        var removedProductId = _faker.Random.Int(1, 1000);
        var keptProductId = _faker.Random.Int(1001, 2000);
        cart.AddItem(removedProductId, _faker.Random.Int(1, 10));
        cart.AddItem(keptProductId, _faker.Random.Int(1, 10));

        // Act
        cart.RemoveItem(removedProductId);

        // Assert
        cart.Items.ShouldHaveSingleItem().ProductId.ShouldBe(keptProductId);
    }

    [Fact]
    public void FindItem_WithProductInCart_ShouldReturnItsLine()
    {
        // Arrange
        var cart = new Cart(_faker.Random.Int(1));
        var productId = _faker.Random.Int(1);
        var quantity = _faker.Random.Int(1, 10);
        cart.AddItem(productId, quantity);

        // Act
        var item = cart.FindItem(productId);

        // Assert
        item.ShouldNotBeNull();
        item.ProductId.ShouldBe(productId);
        item.Quantity.ShouldBe(quantity);
    }

    [Fact]
    public void FindItem_WithUnknownProduct_ShouldReturnNull()
    {
        // Arrange
        var cart = new Cart(_faker.Random.Int(1));
        cart.AddItem(_faker.Random.Int(1, 1000), _faker.Random.Int(1, 10));

        // Act
        var item = cart.FindItem(_faker.Random.Int(1001, 2000));

        // Assert
        item.ShouldBeNull();
    }
}
