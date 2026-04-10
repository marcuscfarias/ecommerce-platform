namespace Ecommerce.Catalog.Application.Categories.GetCategoryById;

public sealed record GetCategoryByIdResult(
    int Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive);
