using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductImage;

internal sealed class GetStorefrontProductImageHandler(
    IProductRepository repository,
    IProductImageStorage imageStorage)
    : IRequestHandler<GetStorefrontProductImageQuery, GetStorefrontProductImageResult>
{
    public async Task<GetStorefrontProductImageResult> Handle(
        GetStorefrontProductImageQuery query, CancellationToken cancellationToken)
    {
        var product = await repository.GetActiveByIdAsync(query.Id, cancellationToken) ??
                      throw new ResourceNotFoundException("Product", query.Id);

        var image = product.ImageKey is null
            ? null
            : await imageStorage.DownloadAsync(product.ImageKey, cancellationToken);

        if (image is null)
        {
            throw new ResourceNotFoundException("Product image", query.Id);
        }

        return new GetStorefrontProductImageResult(image.Content, image.ContentType, image.ContentLength, image.ETag);
    }
}
