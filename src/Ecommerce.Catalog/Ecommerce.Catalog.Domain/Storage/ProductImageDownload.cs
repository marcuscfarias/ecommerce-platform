namespace Ecommerce.Catalog.Domain.Storage;

public sealed record ProductImageDownload(
    Stream Content,
    string ContentType,
    long ContentLength,
    string ETag);
