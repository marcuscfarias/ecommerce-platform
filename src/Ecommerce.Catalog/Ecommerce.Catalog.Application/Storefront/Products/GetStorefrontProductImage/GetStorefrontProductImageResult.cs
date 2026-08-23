namespace Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductImage;

public sealed record GetStorefrontProductImageResult(
    Stream Content,
    string ContentType,
    long ContentLength,
    string ETag);
