namespace Ecommerce.Admin.Web.Services;

public sealed record UserDetail(
    int Id,
    string Email,
    string Name,
    bool IsActive,
    IReadOnlyList<string> Roles);
