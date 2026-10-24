namespace Ecommerce.Shop.Web.Services;

public sealed record AddCartItemRequest(int ProductId, int Quantity);
