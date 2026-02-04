namespace Ecommerce.Common.Domain.BusinessRules;

public static class Ensure
{
    public static void That(IRule rule)
    {
        if (!rule.IsMet())
        {
            throw rule.CreateException();
        }
    }
}