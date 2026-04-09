namespace Ecommerce.Shared.Domain.BusinessRules;

public static class BusinessRule
{
    public static void Validate(IBusinessRule rule)
    {
        if (!rule.IsMet())
            throw rule.CreateException();
    }
}
