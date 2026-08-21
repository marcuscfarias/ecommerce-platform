namespace Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;

internal static class ListStorefrontProductsConsts
{
    public const int SearchMaxLength = 200;

    public const string SortByNameAscending = "name_asc";
    public const string SortByPriceAscending = "price_asc";
    public const string SortByPriceDescending = "price_desc";
    public const string SortByNewest = "newest";

    public static readonly string[] AllowedSortValues =
    [
        SortByNameAscending,
        SortByPriceAscending,
        SortByPriceDescending,
        SortByNewest,
    ];
}
