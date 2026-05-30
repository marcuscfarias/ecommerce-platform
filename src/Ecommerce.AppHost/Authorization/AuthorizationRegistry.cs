using Ecommerce.Auth.Application.Auth.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.AppHost.Authorization;

internal static class AuthorizationRegistry
{
    // Cross-module authorization composition owned by the host: it maps each role to the
    // permissions of every module. It lives here because no single module may know another's
    // permissions. Swap RolePermissionMap for a config/DB-backed implementation here.
    internal static IServiceCollection AddHostAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IRolePermissionMap, RolePermissionMap>();
        return services;
    }
}
