using Ecommerce.Catalog.Application.Categories.GetCategoryById;

namespace Ecommerce.Catalog.Api.Categories.GetCategoryById;

public sealed record GetCategoryByIdRequest
{
    internal static GetCategoryByIdQuery ToQuery(int id) => new(id);
}
