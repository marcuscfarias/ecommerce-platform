namespace Ecommerce.Admin.Web.Services;

public sealed record MeResponse(int Id, string Email, string Name, IReadOnlyList<string> Roles);
