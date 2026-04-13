using Ecommerce.Shared.Application.CQRS;

namespace Ecommerce.Catalog.Application.Categories.ListCategories;

public sealed record ListCategoriesQuery(
    int PageNumber,
    bool? IsActive) : IQuery<ListCategoriesResult>;
