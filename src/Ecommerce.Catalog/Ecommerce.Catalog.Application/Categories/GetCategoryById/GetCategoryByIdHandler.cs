using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Shared.Application.Exceptions;
using MediatR;

namespace Ecommerce.Catalog.Application.Categories.GetCategoryById;

internal sealed class GetCategoryByIdHandler(ICatalogRepository repository)
    : IRequestHandler<GetCategoryByIdQuery, GetCategoryByIdResult>
{
    public async Task<GetCategoryByIdResult> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(query.Id, cancellationToken) ??
                       throw new ResourceNotFoundException("Category", query.Id);

        return new GetCategoryByIdResult(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.IsActive);
    }
}
