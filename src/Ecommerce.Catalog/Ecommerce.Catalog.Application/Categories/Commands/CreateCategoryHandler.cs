using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Domain.BusinessRules;
using MediatR;

namespace Ecommerce.Catalog.Application.Categories.Commands;

internal sealed class CreateCategoryHandler(ICatalogRepository repository) : IRequestHandler<CreateCategoryCommand, int>
{
    public async Task<int> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var exists = await repository.ExistsAsync(command.Name, cancellationToken);
        if (exists)
            throw new BusinessRuleValidationException(CategoryConsts.NameDuplicateError);

        var category = new Category(command.Name, command.Description);

        await repository.Add(category, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
