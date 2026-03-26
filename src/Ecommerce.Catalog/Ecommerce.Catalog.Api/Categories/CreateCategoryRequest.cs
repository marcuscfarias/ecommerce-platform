using Ecommerce.Catalog.Application.Categories.Commands;

namespace Ecommerce.Catalog.Api.Categories;

internal sealed record CreateCategoryRequest(string Name, string? Description)
{
    internal CreateCategoryCommand ToCommand() => new(Name, Description);
}
