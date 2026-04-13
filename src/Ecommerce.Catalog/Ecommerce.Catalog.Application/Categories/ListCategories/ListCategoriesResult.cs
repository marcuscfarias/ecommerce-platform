namespace Ecommerce.Catalog.Application.Categories.ListCategories;

public sealed record ListCategoriesResult(
    IReadOnlyList<ListCategoriesItemResult> Data,
    int Page,
    int TotalCount,
    int TotalPages);

public sealed record ListCategoriesItemResult(
    int Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive);
