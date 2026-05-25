using Ecommerce.Catalog.Application.Categories.UpdateCategory;

namespace Ecommerce.Catalog.Api.Categories.UpdateCategory;

public sealed record UpdateCategoryRequest(string Name, string? Description)
{
    internal UpdateCategoryCommand ToCommand(int id) => new(id, Name, Description);
}
