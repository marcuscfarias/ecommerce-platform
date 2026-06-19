using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Products.SetProductStatus;

internal sealed class SetProductStatusHandler(IProductRepository repository)
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
    }
}
