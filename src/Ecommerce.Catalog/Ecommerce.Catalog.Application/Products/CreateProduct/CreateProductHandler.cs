using Ecommerce.Catalog.Application.Exceptions;
using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.ValueObjects;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Products.CreateProduct;

internal sealed class CreateProductHandler(IProductRepository repository)
    : IRequestHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        if (!await repository.CheckCategoryExistsAsync(command.CategoryId, cancellationToken))
            throw new ResourceNotFoundException("Category", command.CategoryId);

        if (await repository.CheckSkuExistsAsync(command.Sku, ct: cancellationToken))
            throw new SkuConflictException(command.Sku);

        var money = new Money(command.Price);
        var product = new Product(
            command.Name,
            command.Description,
            money,
            command.Sku,
            command.CategoryId,
            command.StockQuantity);

        repository.Add(product);
        await repository.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
