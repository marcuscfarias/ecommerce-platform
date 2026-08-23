namespace Ecommerce.Shop.Web.Services;

public enum StorefrontProductStatus
{
    Found,
    NotFound,
    Failed,
}

// A missing product and an unreachable API are different screens, so the detail
// request reports which one happened instead of collapsing both into null.
public sealed record StorefrontProductResult(StorefrontProductStatus Status, StorefrontProductDetail? Product)
{
    public static StorefrontProductResult Found(StorefrontProductDetail product) =>
        new(StorefrontProductStatus.Found, product);

    public static StorefrontProductResult NotFound() => new(StorefrontProductStatus.NotFound, null);

    public static StorefrontProductResult Failed() => new(StorefrontProductStatus.Failed, null);
}
