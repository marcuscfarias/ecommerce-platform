using Ecommerce.Catalog.Application.Storefront.Products.ListStorefrontProducts;

namespace Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;

public sealed record ListStorefrontProductsResponse(
    IReadOnlyList<ListStorefrontProductsItemResponse> Data,
    int Page,
    int TotalCount,
    int TotalPages)
{
    internal static ListStorefrontProductsResponse FromResult(ListStorefrontProductsResult result) =>
        new(result.Data.Select(i => new ListStorefrontProductsItemResponse(
            i.Id, i.Name, i.Price, i.InStock, i.HasImage)).ToList(),
            result.Page,
            result.TotalCount,
            result.TotalPages);
}

public sealed record ListStorefrontProductsItemResponse(
    int Id,
    string Name,
    decimal Price,
    bool InStock,
    bool HasImage);
