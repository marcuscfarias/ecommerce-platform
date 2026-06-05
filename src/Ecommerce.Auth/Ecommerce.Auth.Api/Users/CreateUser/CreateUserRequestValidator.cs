using Ecommerce.Auth.Api.Users.Validation;
using Ecommerce.Auth.Domain.Entities;
using FluentValidation;

namespace Ecommerce.Auth.Api.Users.CreateUser;

internal sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(UserConsts.EmailMaxLength);

        RuleFor(x => x.Password).ValidPassword();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(UserConsts.NameMaxLength);
    }
}
