using Ecommerce.Catalog.Domain.Repositories;
using MediatR;

namespace Ecommerce.Catalog.Application.Storefront.Categories.ListStorefrontCategories;

internal sealed class ListStorefrontCategoriesHandler(ICatalogRepository repository)
    : IRequestHandler<ListStorefrontCategoriesQuery, ListStorefrontCategoriesResult>
{
    public async Task<ListStorefrontCategoriesResult> Handle(
        ListStorefrontCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await repository.GetActiveAsync(cancellationToken);

        var items = categories
            .Select(c => new ListStorefrontCategoriesItemResult(c.Id, c.Name))
            .ToList();

        return new ListStorefrontCategoriesResult(items);
    }
}
