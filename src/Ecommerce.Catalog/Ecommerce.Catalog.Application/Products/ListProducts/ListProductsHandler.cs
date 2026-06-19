using Ecommerce.Catalog.Domain.Repositories;
using MediatR;

namespace Ecommerce.Catalog.Application.Products.ListProducts;

internal sealed class ListProductsHandler(IProductRepository repository)
    : IRequestHandler<ListProductsQuery, ListProductsResult>
{
    public async Task<ListProductsResult> Handle(
        ListProductsQuery query, CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(
            query.PageNumber,
            query.CategoryId,
            query.IsActive,
            cancellationToken);

        var items = result.Data
            .Select(p => new ListProductsItemResult(p.Id, p.Name, p.Price.Amount, p.IsActive))
            .ToList();

        return new ListProductsResult(items, result.Page, result.TotalCount, result.TotalPages);
    }
}
