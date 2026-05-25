using Ecommerce.Kernel.Domain.BusinessRules;

namespace Ecommerce.Auth.Domain.Rules;

public class AdminCannotBeDeactivatedRule(bool isAdmin) : IBusinessRule
{
    public bool IsMet() => isAdmin is not true;
    public string ErrorMessage => "Admin user cannot be deactivated.";
}
