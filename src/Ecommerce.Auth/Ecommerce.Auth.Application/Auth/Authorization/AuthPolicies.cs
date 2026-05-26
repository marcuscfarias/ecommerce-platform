namespace Ecommerce.Auth.Application.Auth.Authorization;

public static class AuthPolicies
{
    public const string CanManageUsers = nameof(CanManageUsers);
    public const string CanViewUsers = nameof(CanViewUsers);
    public const string CanManageCatalog = nameof(CanManageCatalog);
}
