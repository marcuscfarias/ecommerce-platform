namespace Ecommerce.Catalog.Api.Products.UploadProductImage;

internal static class UploadProductImageConsts
{
    public const long MaxSizeBytes = 2 * 1024 * 1024;

    public static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png"];
}
