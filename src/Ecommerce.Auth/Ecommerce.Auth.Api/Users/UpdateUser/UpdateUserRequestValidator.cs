using Ecommerce.Auth.Domain.Entities;
using FluentValidation;

namespace Ecommerce.Auth.Api.Users.UpdateUser;

internal sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(UserConsts.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(UserConsts.LastNameMaxLength);
    }
}
