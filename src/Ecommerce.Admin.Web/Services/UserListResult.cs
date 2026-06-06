namespace Ecommerce.Admin.Web.Services;

public sealed record UserListResult(
    IReadOnlyList<UserListItem> Data,
    int Page,
    int TotalCount,
    int TotalPages);
