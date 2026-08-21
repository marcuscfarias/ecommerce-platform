using Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductById;

namespace Ecommerce.Catalog.Api.Storefront.Products.GetStorefrontProductById;

public sealed record GetStorefrontProductByIdResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int CategoryId,
    bool InStock,
    bool HasImage)
{
    internal static GetStorefrontProductByIdResponse FromResult(GetStorefrontProductByIdResult result) =>
        new(
            result.Id,
            result.Name,
            result.Description,
            result.Price,
            result.CategoryId,
            result.InStock,
            result.HasImage);
}
