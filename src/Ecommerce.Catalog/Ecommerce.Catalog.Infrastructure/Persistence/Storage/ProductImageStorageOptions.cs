namespace Ecommerce.Catalog.Infrastructure.Persistence.Storage;

internal sealed class ProductImageStorageOptions
{
    public const string SectionName = "ProductImageStorage";
    public string? ServiceUri { get; init; } //production
    public string? ConnectionString { get; init; } //local
    public string ContainerName { get; init; } = "product-images";
}
