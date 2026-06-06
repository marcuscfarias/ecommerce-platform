using Ecommerce.Kernel.Domain.BusinessRules;

namespace Ecommerce.Auth.Application.Users.Rules;

internal class AdminCannotBeModifiedRule(bool isAdmin) : IBusinessRule
{
    public bool IsMet() => isAdmin is not true;
    public string ErrorMessage => "Admin user cannot be modified.";
}
