using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Domain.BusinessRules;
using MediatR;

namespace Ecommerce.Catalog.Application.Categories.CreateCategory;

internal sealed class CreateCategoryHandler(ICatalogRepository repository) : IRequestHandler<CreateCategoryCommand, int>
{
    public async Task<int> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var exists = await repository.ExistsAsync(command.Name, cancellationToken);
        BusinessRule.Validate(new CategoryMustBeUniqueRule(exists));

        var category = new Category(command.Name, command.Slug, command.Description);

        repository.Add(category);
        await repository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
