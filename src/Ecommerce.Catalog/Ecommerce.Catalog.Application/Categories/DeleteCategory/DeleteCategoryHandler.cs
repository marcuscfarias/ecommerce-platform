using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Categories.DeleteCategory;

internal sealed class DeleteCategoryHandler(ICatalogRepository repository)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(command.Id, cancellationToken) ??
                       throw new ResourceNotFoundException("Category", command.Id);

        repository.Remove(category);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
