using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Shared.Application.Exceptions;
using Ecommerce.Shared.Domain.BusinessRules;

namespace Ecommerce.Catalog.Application.Categories.Rules;

public class CategoryMustExistRule(Category? category) : IBusinessRule
{
    public bool IsMet() => category is not null;

    public Exception CreateException() =>
        new ResourceNotFoundException(CategoryConsts.NotFoundError);
}
