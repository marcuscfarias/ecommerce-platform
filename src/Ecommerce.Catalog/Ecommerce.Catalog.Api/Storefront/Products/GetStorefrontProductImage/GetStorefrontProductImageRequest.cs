using Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductImage;

namespace Ecommerce.Catalog.Api.Storefront.Products.GetStorefrontProductImage;

public sealed record GetStorefrontProductImageRequest
{
    internal static GetStorefrontProductImageQuery ToQuery(int id) => new(id);
}
