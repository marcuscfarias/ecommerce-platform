namespace Ecommerce.Common.Domain.BusinessRules;

public static class BusinessRuleValidator
{
    public static void Validate(IBusinessRule businessRule)
    {
        if (!businessRule.IsMet())
        {
            throw businessRule.CreateException(businessRule.Error);
        }
    }
}