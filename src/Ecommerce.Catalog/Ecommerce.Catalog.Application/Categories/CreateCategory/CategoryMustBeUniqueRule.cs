using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Shared.Application.Exceptions;
using Ecommerce.Shared.Domain.BusinessRules;

namespace Ecommerce.Catalog.Application.Categories.CreateCategory;

public class CategoryMustBeUniqueRule(bool exists) : IBusinessRule
{
    public bool IsMet() => !exists;

    public Exception CreateException() =>
        new ResourceAlreadyExistsException(CategoryConsts.NameDuplicateError);
}
