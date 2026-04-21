using Ecommerce.Catalog.Application.Categories.Rules;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Application.Exceptions;
using Ecommerce.Shared.Domain.BusinessRules;
using MediatR;

namespace Ecommerce.Catalog.Application.Categories.UpdateCategory;

internal sealed class UpdateCategoryHandler(ICatalogRepository repository)
    : IRequestHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(command.Id, cancellationToken) ??
                       throw new ResourceNotFoundException("Category", command.Id);

        var slugExists = await repository.CheckSlugExistsAsync(command.Slug, command.Id, cancellationToken);
        BusinessRule.Validate(new CategorySlugMustBeUniqueRule(slugExists));

        category.Update(command.Name, command.Slug, command.Description, command.IsActive);
        repository.Update(category);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
