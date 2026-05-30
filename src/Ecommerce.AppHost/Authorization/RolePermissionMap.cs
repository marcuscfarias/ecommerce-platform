using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.Application.Auth.Authorization;
using Ecommerce.Auth.Domain.Enums;

namespace Ecommerce.AppHost.Authorization;

// The composition root is the only place allowed to know every module's permissions,
// so the role->permission map lives here. Hardcoded for now; swappable for config/DB
// later behind IRolePermissionMap without touching any module.
internal sealed class RolePermissionMap : IRolePermissionMap
{
    private static readonly Dictionary<RoleName, string[]> Map = new()
    {
        [RoleName.Admin] = [AuthPermissions.ManageUsers, AuthPermissions.ViewUsers],
        [RoleName.Owner] = [AuthPermissions.ViewUsers],
        [RoleName.Manager] = [],
    };

    public IReadOnlyCollection<string> ResolvePermissions(IEnumerable<RoleName> roles) =>
        roles
            .SelectMany(role => Map.TryGetValue(role, out var permissions) ? permissions : [])
            .Distinct()
            .ToArray();
}
