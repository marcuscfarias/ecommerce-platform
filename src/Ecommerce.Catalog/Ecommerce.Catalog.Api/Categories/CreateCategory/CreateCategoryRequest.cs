using Ecommerce.Catalog.Application.Categories.CreateCategory;

namespace Ecommerce.Catalog.Api.Categories.CreateCategory;

public sealed record CreateCategoryRequest(string Name, string Slug, string? Description = null)
{
    internal CreateCategoryCommand ToCommand() => new(Name, Slug, Description);
}
