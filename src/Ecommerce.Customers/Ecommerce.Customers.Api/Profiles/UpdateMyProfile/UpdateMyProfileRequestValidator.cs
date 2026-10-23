using Ecommerce.Customers.Domain.Entities;
using FluentValidation;

namespace Ecommerce.Customers.Api.Profiles.UpdateMyProfile;

internal sealed class UpdateMyProfileRequestValidator : AbstractValidator<UpdateMyProfileRequest>
{
    private const string E164PhonePattern = @"^\+[1-9]\d{7,14}$";

    public UpdateMyProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(CustomerConsts.DisplayNameMaxLength);

        RuleFor(x => x.Phone)
            .Matches(E164PhonePattern)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone must be in E.164 format, e.g. +5511999998888.");
    }
}
