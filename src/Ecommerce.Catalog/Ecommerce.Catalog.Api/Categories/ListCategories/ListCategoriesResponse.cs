using Ecommerce.Catalog.Application.Categories.ListCategories;

namespace Ecommerce.Catalog.Api.Categories.ListCategories;

public sealed record ListCategoriesResponse(
    IReadOnlyList<ListCategoriesItemResponse> Data,
    int Page,
    int TotalCount,
    int TotalPages)
{
    internal static ListCategoriesResponse FromResult(ListCategoriesResult result) =>
        new(result.Data.Select(i => new ListCategoriesItemResponse(
            i.Id, i.Name, i.Slug, i.Description, i.IsActive)).ToList(),
            result.Page,
            result.TotalCount,
            result.TotalPages);
}

public sealed record ListCategoriesItemResponse(
    int Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive);
