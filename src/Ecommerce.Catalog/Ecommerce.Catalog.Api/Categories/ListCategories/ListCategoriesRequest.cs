using Ecommerce.Catalog.Application.Categories.ListCategories;

namespace Ecommerce.Catalog.Api.Categories.ListCategories;

public sealed record ListCategoriesRequest(
    int PageNumber = 1,
    bool? IsActive = null)
{
    internal ListCategoriesQuery ToQuery() => new(PageNumber, IsActive);
}
