using Ecommerce.Kernel.Domain.BusinessRules;

namespace Ecommerce.Auth.Application.Users.Rules;

internal class EmailMustBeUniqueRule(bool exists) : IBusinessRule
{
    public bool IsMet() => exists is not true;
    public string ErrorMessage => "A user with this email already exists.";
}
