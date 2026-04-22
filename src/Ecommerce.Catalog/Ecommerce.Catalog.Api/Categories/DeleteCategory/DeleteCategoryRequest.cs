using Ecommerce.Catalog.Application.Categories.DeleteCategory;

namespace Ecommerce.Catalog.Api.Categories.DeleteCategory;

public sealed record DeleteCategoryRequest
{
    internal DeleteCategoryCommand ToCommand(int id) => new(id);
}
