using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductById;

internal sealed class GetStorefrontProductByIdHandler(IProductRepository repository)
    : IRequestHandler<GetStorefrontProductByIdQuery, GetStorefrontProductByIdResult>
{
    public async Task<GetStorefrontProductByIdResult> Handle(
        GetStorefrontProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await repository.GetActiveByIdAsync(query.Id, cancellationToken) ??
                      throw new ResourceNotFoundException("Product", query.Id);

        return new GetStorefrontProductByIdResult(
            product.Id,
            product.Name,
            product.Description,
            product.Price.Amount,
            product.CategoryId,
            product.StockQuantity > 0,
            product.ImageKey is not null);
    }
}
