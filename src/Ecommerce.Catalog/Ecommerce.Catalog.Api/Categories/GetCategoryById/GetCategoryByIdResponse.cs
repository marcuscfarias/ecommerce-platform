using Ecommerce.Catalog.Application.Categories.GetCategoryById;

namespace Ecommerce.Catalog.Api.Categories.GetCategoryById;

public sealed record GetCategoryByIdResponse(
    int Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive)
{
    internal static GetCategoryByIdResponse FromResult(GetCategoryByIdResult result) =>
        new(result.Id, result.Name, result.Slug, result.Description, result.IsActive);
}
