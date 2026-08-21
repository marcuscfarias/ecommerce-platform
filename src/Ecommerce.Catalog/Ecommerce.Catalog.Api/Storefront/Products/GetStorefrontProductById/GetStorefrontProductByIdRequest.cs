using Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductById;

namespace Ecommerce.Catalog.Api.Storefront.Products.GetStorefrontProductById;

public sealed record GetStorefrontProductByIdRequest
{
    internal static GetStorefrontProductByIdQuery ToQuery(int id) => new(id);
}
