using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Products.GetProductImage;

internal sealed class GetProductImageHandler(
    IProductRepository repository,
    IProductImageStorage imageStorage) : IRequestHandler<GetProductImageQuery, GetProductImageResult>
{
    public async Task<GetProductImageResult> Handle(GetProductImageQuery query, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(query.Id, cancellationToken) ??
                      throw new ResourceNotFoundException("Product", query.Id);

        var image = product.ImageKey is null
            ? null
            : await imageStorage.DownloadAsync(product.ImageKey, cancellationToken);

        if (image is null)
        {
            throw new ResourceNotFoundException("Product image", query.Id);
        }

        return new GetProductImageResult(image.Content, image.ContentType);
    }
}
