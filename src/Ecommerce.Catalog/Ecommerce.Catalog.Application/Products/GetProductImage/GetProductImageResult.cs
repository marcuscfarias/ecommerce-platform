namespace Ecommerce.Catalog.Application.Products.GetProductImage;

public sealed record GetProductImageResult(
    Stream Content,
    string ContentType,
    long ContentLength,
    string ETag);
