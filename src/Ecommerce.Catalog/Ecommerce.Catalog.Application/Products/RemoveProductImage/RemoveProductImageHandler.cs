using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Catalog.Application.Products.RemoveProductImage;

internal sealed partial class RemoveProductImageHandler(
    IProductRepository repository,
    IProductImageStorage imageStorage,
    ILogger<RemoveProductImageHandler> logger) : IRequestHandler<RemoveProductImageCommand>
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

        LogImageRemoved(logger, product.Id);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Product {ProductId} image removed")]
    private static partial void LogImageRemoved(ILogger logger, int productId);
}
