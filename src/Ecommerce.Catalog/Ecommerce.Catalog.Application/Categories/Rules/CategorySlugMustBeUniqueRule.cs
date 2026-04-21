using Ecommerce.Shared.Domain.BusinessRules;

namespace Ecommerce.Catalog.Application.Categories.Rules;

internal class CategorySlugMustBeUniqueRule(bool exists) : IBusinessRule
{
    public bool IsMet() => exists is not true;
    public string ErrorMessage => "A category with this slug already exists.";
}
