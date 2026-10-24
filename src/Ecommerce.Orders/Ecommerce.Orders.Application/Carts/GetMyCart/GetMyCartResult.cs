namespace Ecommerce.Orders.Application.Carts.GetMyCart;

public sealed record GetMyCartResult(
    IReadOnlyList<GetMyCartItemResult> Items,
    int TotalQuantity,
    decimal Total);

public sealed record GetMyCartItemResult(
    int ProductId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal,
    bool HasImage,
    CartItemStatus Status,
    int? AvailableQuantity);
