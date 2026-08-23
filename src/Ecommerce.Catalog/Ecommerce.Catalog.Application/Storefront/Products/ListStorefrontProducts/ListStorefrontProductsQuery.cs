using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Storefront.Products.ListStorefrontProducts;

public sealed record ListStorefrontProductsQuery(
    int PageNumber,
    int? CategoryId,
    string? Search,
    StorefrontProductSort Sort) : IQuery<ListStorefrontProductsResult>;
