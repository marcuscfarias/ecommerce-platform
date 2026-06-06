using Ecommerce.Auth.Domain.Enums;
using FluentValidation;

namespace Ecommerce.Auth.Api.Users.SetUserRoles;

internal sealed class SetUserRolesRequestValidator : AbstractValidator<SetUserRolesRequest>
{
    public SetUserRolesRequestValidator()
    {
        RuleFor(x => x.Roles).NotEmpty();
        RuleForEach(x => x.Roles)
            .Must(BeAKnownRole)
            .WithMessage("'Roles' contains an unknown role name.");
    }

    private static bool BeAKnownRole(string roleName) => Enum.GetNames<RoleName>().Contains(roleName);
}
