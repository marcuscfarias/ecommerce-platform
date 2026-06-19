namespace Ecommerce.Catalog.Application.Products.ListProducts;

public sealed record ListProductsResult(
    IReadOnlyList<ListProductsItemResult> Data,
    int Page,
    int TotalCount,
    int TotalPages);

public sealed record ListProductsItemResult(
    int Id,
    string Name,
    decimal Price,
    bool IsActive);
