using Ecommerce.Catalog.Application.Exceptions;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Products.UpdateProduct;

internal sealed class UpdateProductHandler(IProductRepository repository)
    : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(command.Id, cancellationToken) ??
                      throw new ResourceNotFoundException("Product", command.Id);

        if (!await repository.CheckCategoryExistsAsync(command.CategoryId, cancellationToken))
            throw new ResourceNotFoundException("Category", command.CategoryId);

        if (await repository.CheckSkuExistsAsync(command.Sku, command.Id, cancellationToken))
            throw new SkuConflictException(command.Sku);

        var money = new Money(command.Price);
        product.Update(
            command.Name,
            command.Description,
            money,
            command.Sku,
            command.CategoryId,
            command.StockQuantity);

        repository.Update(product);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
