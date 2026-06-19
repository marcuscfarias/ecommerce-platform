namespace Ecommerce.Admin.Web.Services;

public sealed record ProductListResult(
    IReadOnlyList<ProductListItem> Data,
    int Page,
    int TotalCount,
    int TotalPages);
