using Ecommerce.Auth.Api.Users.Validation;
using FluentValidation;

namespace Ecommerce.Auth.Api.Users.ResetUserPassword;

internal sealed class ResetUserPasswordRequestValidator : AbstractValidator<ResetUserPasswordRequest>
{
    public ResetUserPasswordRequestValidator()
    {
        RuleFor(x => x.Password).ValidPassword();
    }
}
