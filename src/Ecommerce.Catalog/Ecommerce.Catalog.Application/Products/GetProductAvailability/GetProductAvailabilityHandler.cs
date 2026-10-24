using Ecommerce.Catalog.Domain.Repositories;
using MediatR;

namespace Ecommerce.Catalog.Application.Products.GetProductAvailability;

internal sealed class GetProductAvailabilityHandler(IProductRepository repository)
    : IRequestHandler<GetProductAvailabilityQuery, GetProductAvailabilityResult>
{
    public async Task<GetProductAvailabilityResult> Handle(
        GetProductAvailabilityQuery query, CancellationToken cancellationToken)
    {
        var products = await repository.GetByIdsAsync(query.ProductIds, cancellationToken);

        var items = products
            .Select(p => new ProductAvailability(
                p.Id,
                p.Name,
                p.Price.Amount,
                p.StockQuantity,
                p.IsActive,
                p.ImageKey is not null))
            .ToList();

        return new GetProductAvailabilityResult(items);
    }
}
