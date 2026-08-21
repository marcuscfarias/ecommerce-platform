namespace Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;

public sealed record ListStorefrontProductsRequest(
    int PageNumber = 1,
    int? CategoryId = null,
    string? Search = null,
    string? Sort = null);
