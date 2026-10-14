using Ecommerce.Auth.Api.Users.Validation;
using Ecommerce.Auth.Domain.Entities;
using FluentValidation;

namespace Ecommerce.Auth.Api.Auth.Register;

internal sealed class RegisterCustomerRequestValidator : AbstractValidator<RegisterCustomerRequest>
{
    public RegisterCustomerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(UserConsts.NameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(UserConsts.EmailMaxLength);

        RuleFor(x => x.Password).ValidPassword();
    }
}
