namespace Ecommerce.Admin.Web.Services;

public sealed record CategoryListResult(
    IReadOnlyList<CategoryListItem> Data,
    int Page,
    int TotalCount,
    int TotalPages);
