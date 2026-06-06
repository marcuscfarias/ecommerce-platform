namespace Ecommerce.Admin.Web.Services;

public sealed record SetUserRolesRequest(IReadOnlyList<string> Roles);
