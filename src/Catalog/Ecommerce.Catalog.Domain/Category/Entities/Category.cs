using Ecommerce.Common.Domain.BusinessRules;
using Ecommerce.Common.Domain.BusinessRules.Rules;
using Ecommerce.Common.Domain.Entities;

namespace Ecommerce.Catalog.Domain.Category.Entities;

public class Category : BaseEntity
{
    public Category(string name)
    {
        BusinessRuleValidator.Validate(new StringNotEmptyBusinessRule(name, nameof(Name)));
        BusinessRuleValidator.Validate(new StringMaxLengthBusinessRule(name, CategoryConstants.NameMaxLength, nameof(Name)));

        Name = name;
    }

    public string Name { get; private set; }
}