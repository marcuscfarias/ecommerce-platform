using Ecommerce.Kernel.Domain.Exceptions;

namespace Ecommerce.Kernel.Domain.BusinessRules;

public static class BusinessRule
{
    public static void Validate(IBusinessRule rule)
    {
        if (!rule.IsMet())
            throw new BusinessRuleValidationException(rule.ErrorMessage);
    }
}
