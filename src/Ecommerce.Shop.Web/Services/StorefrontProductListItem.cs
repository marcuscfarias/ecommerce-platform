namespace Ecommerce.Shop.Web.Services;

public sealed record StorefrontProductListItem(
    int Id,
    string Name,
    decimal Price,
    bool InStock,
    bool HasImage);
