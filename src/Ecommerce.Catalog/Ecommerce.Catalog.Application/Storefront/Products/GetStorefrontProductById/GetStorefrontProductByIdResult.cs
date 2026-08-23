namespace Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductById;

public sealed record GetStorefrontProductByIdResult(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int CategoryId,
    bool InStock,
    bool HasImage);
