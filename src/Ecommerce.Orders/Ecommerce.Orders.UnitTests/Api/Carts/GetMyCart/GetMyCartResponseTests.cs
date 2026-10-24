using Ecommerce.Orders.Api.Carts.GetMyCart;
using Ecommerce.Orders.Application.Carts;
using Ecommerce.Orders.Application.Carts.GetMyCart;

namespace Ecommerce.Orders.UnitTests.Api.Carts.GetMyCart;

public class GetMyCartResponseTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void FromResult_WhenCartHasLine_ShouldCopyEveryFieldAndTotals()
    {
        // Arrange
        var item = new GetMyCartItemResult(
            _faker.Random.Int(1),
            _faker.Commerce.ProductName(),
            UnitPrice: 30.00m,
            Quantity: 3,
            Subtotal: 90.00m,
            HasImage: true,
            CartItemStatus.InsufficientStock,
            AvailableQuantity: 1);
        var result = new GetMyCartResult([item], TotalQuantity: 3, Total: 0m);

        // Act
        var response = GetMyCartResponse.FromResult(result);

        // Assert
        response.Items.ShouldHaveSingleItem().ShouldBe(new GetMyCartItemResponse(
            item.ProductId,
            item.Name,
            UnitPrice: 30.00m,
            Quantity: 3,
            Subtotal: 90.00m,
            HasImage: true,
            Status: "insufficientStock",
            AvailableQuantity: 1));
        response.TotalQuantity.ShouldBe(3);
        response.Total.ShouldBe(0m);
    }

    [Theory]
    [InlineData(CartItemStatus.Available, "available")]                 // buyable line
    [InlineData(CartItemStatus.OutOfStock, "outOfStock")]               // stock is zero
    [InlineData(CartItemStatus.InsufficientStock, "insufficientStock")] // quantity above stock
    [InlineData(CartItemStatus.Unavailable, "unavailable")]             // inactive or missing product
    public void FromResult_WhenLineHasStatus_ShouldSerializeItInCamelCase(CartItemStatus status, string expected)
    {
        // Arrange
        var item = new GetMyCartItemResult(
            _faker.Random.Int(1),
            _faker.Commerce.ProductName(),
            UnitPrice: 10.00m,
            Quantity: 1,
            Subtotal: 10.00m,
            HasImage: false,
            status,
            AvailableQuantity: null);
        var result = new GetMyCartResult([item], TotalQuantity: 1, Total: 10.00m);

        // Act
        var response = GetMyCartResponse.FromResult(result);

        // Assert
        response.Items.ShouldHaveSingleItem().Status.ShouldBe(expected);
    }
}
