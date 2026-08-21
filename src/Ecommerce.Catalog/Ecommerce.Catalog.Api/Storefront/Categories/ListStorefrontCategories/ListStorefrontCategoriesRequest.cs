using Ecommerce.Catalog.Application.Storefront.Categories.ListStorefrontCategories;

namespace Ecommerce.Catalog.Api.Storefront.Categories.ListStorefrontCategories;

public sealed record ListStorefrontCategoriesRequest
{
    internal static ListStorefrontCategoriesQuery ToQuery() => new();
}
