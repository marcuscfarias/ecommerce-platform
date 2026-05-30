namespace Ecommerce.Auth.Api.Authorization;

// Permissions owned by the Auth module. These strings are the Auth module's
// vocabulary and are referenced only here and by the host's role->permission map.
public static class AuthPermissions
{
    public const string ManageUsers = "users:manage";
    public const string ViewUsers = "users:view";
}
