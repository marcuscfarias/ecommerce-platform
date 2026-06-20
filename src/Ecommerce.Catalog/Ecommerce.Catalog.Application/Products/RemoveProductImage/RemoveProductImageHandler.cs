using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Products.RemoveProductImage;

internal sealed class RemoveProductImageHandler(
    IProductRepository repository,
    IProductImageStorage imageStorage) : IRequestHandler<RemoveProductImageCommand>
{
    public async Task Handle(RemoveProductImageCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(command.Id, cancellationToken) ??
                      throw new ResourceNotFoundException("Product", command.Id);

        if (product.ImageKey is null)
            return;

        await imageStorage.DeleteAsync(product.ImageKey, cancellationToken);
        product.RemoveImage();

        repository.Update(product);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
