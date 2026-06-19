using Ecommerce.Catalog.Application.Products.GetProductById;

namespace Ecommerce.Catalog.Api.Products.GetProductById;

public sealed record GetProductByIdRequest
{
    internal static GetProductByIdQuery ToQuery(int id) => new(id);
}
