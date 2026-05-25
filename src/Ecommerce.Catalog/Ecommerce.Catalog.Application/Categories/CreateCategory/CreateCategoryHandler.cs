using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using MediatR;

namespace Ecommerce.Catalog.Application.Categories.CreateCategory;

internal sealed class CreateCategoryHandler(ICatalogRepository repository) : IRequestHandler<CreateCategoryCommand, int>
{
    public async Task<int> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = new Category(command.Name, command.Description);

        repository.Add(category);
        await repository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
