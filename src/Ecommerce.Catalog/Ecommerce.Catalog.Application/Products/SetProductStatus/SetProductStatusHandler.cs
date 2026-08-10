using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Catalog.Application.Products.SetProductStatus;

internal sealed partial class SetProductStatusHandler(
    IProductRepository repository,
    ILogger<SetProductStatusHandler> logger)
    : IRequestHandler<SetProductStatusCommand>
{
    public async Task Handle(SetProductStatusCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(command.Id, cancellationToken) ??
                      throw new ResourceNotFoundException("Product", command.Id);

        if (command.IsActive == product.IsActive)
            return;

        if (command.IsActive)
            product.Activate();
        else
            product.Deactivate();

        repository.Update(product);
        await repository.SaveChangesAsync(cancellationToken);

        LogStatusChanged(logger, product.Id, product.IsActive);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Product {ProductId} activation set to {IsActive}")]
    private static partial void LogStatusChanged(ILogger logger, int productId, bool isActive);
}
