using Ecommerce.Auth.Domain.Enums;

namespace Ecommerce.Auth.Application.Auth.Authorization;

// Expands a user's roles into the permission strings stamped on the token at login.
// Auth defines the contract in terms of what it owns (RoleName) and opaque strings;
// the composition root — the only place allowed to know every module's permissions —
// provides the implementation. This keeps Auth unaware of other modules.
public interface IRolePermissionMap
{
    IReadOnlyCollection<string> ResolvePermissions(IEnumerable<RoleName> roles);
}
