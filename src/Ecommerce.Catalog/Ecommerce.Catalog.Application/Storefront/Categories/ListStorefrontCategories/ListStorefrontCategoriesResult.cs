namespace Ecommerce.Catalog.Application.Storefront.Categories.ListStorefrontCategories;

public sealed record ListStorefrontCategoriesResult(IReadOnlyList<ListStorefrontCategoriesItemResult> Data);

public sealed record ListStorefrontCategoriesItemResult(int Id, string Name);
