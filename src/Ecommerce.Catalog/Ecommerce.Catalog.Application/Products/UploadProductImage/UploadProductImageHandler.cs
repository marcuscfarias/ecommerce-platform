using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Products.UploadProductImage;

internal sealed class UploadProductImageHandler(
    IProductRepository repository,
    IProductImageStorage imageStorage) : IRequestHandler<UploadProductImageCommand>
{
    public async Task Handle(UploadProductImageCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(command.Id, cancellationToken) ??
                      throw new ResourceNotFoundException("Product", command.Id);

        if (product.ImageKey is not null)
            await imageStorage.DeleteAsync(product.ImageKey, cancellationToken);

        var imageKey = await imageStorage.UploadAsync(command.Content, command.ContentType, cancellationToken);
        product.SetImageKey(imageKey);

        repository.Update(product);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
