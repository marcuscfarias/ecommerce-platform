using Ecommerce.Catalog.Application.Products.ListProducts;

namespace Ecommerce.Catalog.Api.Products.ListProducts;

public sealed record ListProductsResponse(
    IReadOnlyList<ListProductsItemResponse> Data,
    int Page,
    int TotalCount,
    int TotalPages)
{
    internal static ListProductsResponse FromResult(ListProductsResult result) =>
        new(result.Data.Select(i => new ListProductsItemResponse(
            i.Id, i.Name, i.Price, i.IsActive)).ToList(),
            result.Page,
            result.TotalCount,
            result.TotalPages);
}

public sealed record ListProductsItemResponse(
    int Id,
    string Name,
    decimal Price,
    bool IsActive);
