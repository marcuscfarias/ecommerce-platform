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

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(UserConsts.PasswordMinLength)
            .MaximumLength(UserConsts.PasswordHashMaxLength)
            .Matches("(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])")
            .WithMessage("'Password' must contain at least one lowercase letter, one uppercase letter, and one digit.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(UserConsts.NameMaxLength);
    }
}
