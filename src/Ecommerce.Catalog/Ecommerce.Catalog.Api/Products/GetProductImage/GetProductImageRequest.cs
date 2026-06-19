using Ecommerce.Catalog.Application.Products.GetProductImage;

namespace Ecommerce.Catalog.Api.Products.GetProductImage;

public sealed record GetProductImageRequest
{
    internal static GetProductImageQuery ToQuery(int id) => new(id);
}
