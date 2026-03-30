using Ecommerce.Catalog.Application.Categories.Commands;

namespace Ecommerce.Catalog.Api.Categories;

public sealed record CreateCategoryRequest(string Name, string? Description)
{
    internal CreateCategoryCommand ToCommand() => new(Name, Description);
}
