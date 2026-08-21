namespace Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;

public sealed record ListStorefrontProductsResponse(
    IReadOnlyList<ListStorefrontProductsItemResponse> Data,
    int Page,
    int TotalCount,
    int TotalPages);

public sealed record ListStorefrontProductsItemResponse(
    int Id,
    string Name,
    decimal Price,
    bool InStock,
    bool HasImage);
