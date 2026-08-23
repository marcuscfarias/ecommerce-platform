using Ecommerce.Catalog.Domain.Repositories;
using MediatR;

namespace Ecommerce.Catalog.Application.Storefront.Products.ListStorefrontProducts;

internal sealed class ListStorefrontProductsHandler(IProductRepository repository)
    : IRequestHandler<ListStorefrontProductsQuery, ListStorefrontProductsResult>
{
    public async Task<ListStorefrontProductsResult> Handle(
        ListStorefrontProductsQuery query, CancellationToken cancellationToken)
    {
        var result = await repository.GetStorefrontPageAsync(
            query.PageNumber,
            query.CategoryId,
            query.Search,
            query.Sort,
            cancellationToken);

        var items = result.Data
            .Select(p => new ListStorefrontProductsItemResult(
                p.Id,
                p.Name,
                p.Price.Amount,
                p.StockQuantity > 0,
                p.ImageKey is not null))
            .ToList();

        return new ListStorefrontProductsResult(items, result.Page, result.TotalCount, result.TotalPages);
    }
}
