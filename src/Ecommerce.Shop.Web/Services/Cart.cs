namespace Ecommerce.Shop.Web.Services;

public sealed record Cart(IReadOnlyList<CartLine> Items, int TotalQuantity, decimal Total);
