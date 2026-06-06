using Ecommerce.Auth.Application.Users.SetUserRoles;
using Ecommerce.Auth.Domain.Enums;

namespace Ecommerce.Auth.Api.Users.SetUserRoles;

public sealed record SetUserRolesRequest(IReadOnlyCollection<string> Roles)
{
    internal SetUserRolesCommand ToCommand(int id) =>
        new(id, Roles.Select(Enum.Parse<RoleName>).Distinct().ToArray());
}
