namespace Ecommerce.Catalog.Application.Storefront.Products.ListStorefrontProducts;

public sealed record ListStorefrontProductsResult(
    IReadOnlyList<ListStorefrontProductsItemResult> Data,
    int Page,
    int TotalCount,
    int TotalPages);

public sealed record ListStorefrontProductsItemResult(
    int Id,
    string Name,
    decimal Price,
    bool InStock,
    bool HasImage);
