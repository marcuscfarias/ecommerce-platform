namespace Ecommerce.Catalog.Api.Storefront.Products.GetStorefrontProductById;

public sealed record GetStorefrontProductByIdResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int CategoryId,
    bool InStock,
    bool HasImage);
