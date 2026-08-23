namespace Ecommerce.Shop.Web.Services;

public sealed record StorefrontProductListResult(
    IReadOnlyList<StorefrontProductListItem> Data,
    int Page,
    int TotalCount,
    int TotalPages);
