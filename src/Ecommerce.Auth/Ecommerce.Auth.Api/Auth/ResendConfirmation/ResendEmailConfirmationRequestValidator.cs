using Ecommerce.Auth.Domain.Entities;
using FluentValidation;

namespace Ecommerce.Auth.Api.Auth.ResendConfirmation;

internal sealed class ResendEmailConfirmationRequestValidator : AbstractValidator<ResendEmailConfirmationRequest>
{
    public ResendEmailConfirmationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(UserConsts.EmailMaxLength);
    }
}
