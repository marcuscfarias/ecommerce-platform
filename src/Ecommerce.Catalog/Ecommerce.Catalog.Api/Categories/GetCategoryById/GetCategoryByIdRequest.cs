using Ecommerce.Catalog.Application.Categories.GetCategoryById;

namespace Ecommerce.Catalog.Api.Categories.GetCategoryById;

public sealed record GetCategoryByIdRequest
{
    internal GetCategoryByIdQuery ToQuery(int id) => new(id);
}
