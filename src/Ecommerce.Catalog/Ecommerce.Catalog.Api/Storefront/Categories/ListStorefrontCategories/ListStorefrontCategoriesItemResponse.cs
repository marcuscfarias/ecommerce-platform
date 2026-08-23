using Ecommerce.Catalog.Application.Storefront.Categories.ListStorefrontCategories;

namespace Ecommerce.Catalog.Api.Storefront.Categories.ListStorefrontCategories;

public sealed record ListStorefrontCategoriesItemResponse(
    int Id,
    string Name)
{
    internal static IReadOnlyList<ListStorefrontCategoriesItemResponse> FromResult(
        ListStorefrontCategoriesResult result) =>
        result.Data.Select(c => new ListStorefrontCategoriesItemResponse(c.Id, c.Name)).ToList();
}
