namespace Ecommerce.Catalog.Api.Authorization;

// Permissions owned by the Catalog module. These strings are the Catalog module's
// vocabulary and are referenced only here and by the host's role->permission map.
public static class CatalogPermissions
{
    public const string Manage = "catalog:manage";
    public const string View = "catalog:view";
}
