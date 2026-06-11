using Ecommerce.Kernel.Domain.BusinessRules;

namespace Ecommerce.Auth.Application.Users.Rules;

internal class OnlyOneAdminRule(bool roleIsAdmin) : IBusinessRule
{
    public bool IsMet() => roleIsAdmin is not true;

    public string ErrorMessage => "Cannot create more than one admin user.";
}
