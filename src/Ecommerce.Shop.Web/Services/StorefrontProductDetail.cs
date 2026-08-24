namespace Ecommerce.Shop.Web.Services;

public sealed record StorefrontProductDetail(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int CategoryId,
    bool InStock,
    bool HasImage);
