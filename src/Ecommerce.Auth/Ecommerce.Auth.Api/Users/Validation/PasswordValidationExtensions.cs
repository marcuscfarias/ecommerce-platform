using Ecommerce.Auth.Domain.Entities;
using FluentValidation;

namespace Ecommerce.Auth.Api.Users.Validation;

internal static class PasswordValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .NotEmpty()
            .MinimumLength(UserConsts.PasswordMinLength)
            .MaximumLength(UserConsts.PasswordHashMaxLength)
            .Matches("(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])")
            .WithMessage("'Password' must contain at least one lowercase letter, one uppercase letter, and one digit.");
}
