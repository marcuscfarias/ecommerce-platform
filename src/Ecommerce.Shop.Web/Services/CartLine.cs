namespace Ecommerce.Shop.Web.Services;

public sealed record CartLine(
    int ProductId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal,
    bool HasImage,
    CartLineStatus Status,
    int? AvailableQuantity);
