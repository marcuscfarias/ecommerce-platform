using Ecommerce.Catalog.Application.Categories.UpdateCategory;

namespace Ecommerce.Catalog.Api.Categories.UpdateCategory;

public sealed record UpdateCategoryRequest(string Name, string Slug, bool IsActive, string? Description)
{
    internal UpdateCategoryCommand ToCommand(int id) => new(id, Name, Slug, Description, IsActive);
}