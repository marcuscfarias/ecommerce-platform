using Ecommerce.Catalog.Application.Products.ListProducts;

namespace Ecommerce.Catalog.Api.Products.ListProducts;

public sealed record ListProductsRequest(
    int PageNumber = 1,
    int? CategoryId = null,
    bool? IsActive = null)
{
    internal ListProductsQuery ToQuery() => new(PageNumber, CategoryId, IsActive);
}
