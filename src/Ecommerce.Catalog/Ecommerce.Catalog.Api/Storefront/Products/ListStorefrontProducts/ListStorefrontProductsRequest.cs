using Ecommerce.Catalog.Application.Storefront.Products.ListStorefrontProducts;
using Ecommerce.Catalog.Domain.Repositories;

namespace Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;

public sealed record ListStorefrontProductsRequest(
    int PageNumber = 1,
    int? CategoryId = null,
    string? Search = null,
    string? Sort = null)
{
    internal ListStorefrontProductsQuery ToQuery() =>
        new(PageNumber, CategoryId, Search, ToSort(Sort));

    private static StorefrontProductSort ToSort(string? sort) => sort switch
    {
        ListStorefrontProductsConsts.SortByPriceAscending => StorefrontProductSort.PriceAscending,
        ListStorefrontProductsConsts.SortByPriceDescending => StorefrontProductSort.PriceDescending,
        ListStorefrontProductsConsts.SortByNewest => StorefrontProductSort.Newest,
        _ => StorefrontProductSort.NameAscending,
    };
}
