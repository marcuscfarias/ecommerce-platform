using Ecommerce.Catalog.Domain.Entities;
using Ecommerce.Shared.Application.Exceptions;
using Ecommerce.Shared.Domain.BusinessRules;

namespace Ecommerce.Catalog.Application.Categories.UpdateCategory;

public class CategorySlugMustBeUniqueRule(bool exists) : IBusinessRule
{
    public bool IsMet() => exists is false;

    public Exception CreateException() =>
        new ResourceAlreadyExistsException(CategoryConsts.SlugDuplicateError);
}
