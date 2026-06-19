using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Products.ListProducts;

public sealed record ListProductsQuery(
    int PageNumber,
    int? CategoryId,
    bool? IsActive) : IQuery<ListProductsResult>;
