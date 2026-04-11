using Ecommerce.Catalog.Application.Categories.UpdateCategory;

namespace Ecommerce.Catalog.Api.Categories.UpdateCategory;

public sealed record UpdateCategoryRequest(string Name, string Slug, string? Description, bool IsActive)
{
    internal UpdateCategoryCommand ToCommand(int id) => new(id, Name, Slug, Description, IsActive);
}
