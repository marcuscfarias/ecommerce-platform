using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Kernel.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Categories.DeleteCategory;

internal sealed class DeleteCategoryHandler(ICatalogRepository repository)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(command.Id, cancellationToken) ??
                       throw new ResourceNotFoundException("Category", command.Id);

        category.Deactivate();
        repository.Update(category);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
