using Ecommerce.Catalog.Domain.Repositories;
using MediatR;

namespace Ecommerce.Catalog.Application.Categories.ListCategories;

internal sealed class ListCategoriesHandler(ICatalogRepository repository)
    : IRequestHandler<ListCategoriesQuery, ListCategoriesResult>
{
    public async Task<ListCategoriesResult> Handle(
        ListCategoriesQuery query, CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(
            query.PageNumber,
            query.IsActive,
            cancellationToken);

        var items = result.Data
            .Select(c => new ListCategoriesItemResult(
                c.Id, c.Name, c.Slug, c.Description, c.IsActive))
            .ToList();

        return new ListCategoriesResult(items, result.Page, result.TotalCount, result.TotalPages);
    }
}
