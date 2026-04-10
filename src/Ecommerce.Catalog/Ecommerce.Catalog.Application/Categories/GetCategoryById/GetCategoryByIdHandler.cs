using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Domain.BusinessRules;
using MediatR;

namespace Ecommerce.Catalog.Application.Categories.GetCategoryById;

internal sealed class GetCategoryByIdHandler(ICatalogRepository repository)
    : IRequestHandler<GetCategoryByIdQuery, GetCategoryByIdResult>
{
    public async Task<GetCategoryByIdResult> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(query.Id, cancellationToken);
        BusinessRule.Validate(new CategoryMustExistRule(category));

        return new GetCategoryByIdResult(
            category!.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.IsActive);
    }
}
