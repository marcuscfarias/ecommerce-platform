using Ecommerce.Shared.Domain.BusinessRules;

namespace Ecommerce.Catalog.Application.Categories.Rules;

public class CategoryNameMustBeUniqueRule(bool exists) : IBusinessRule
{
    public bool IsMet() => exists is not true;
    public string ErrorMessage => "A category with this name already exists.";
}
