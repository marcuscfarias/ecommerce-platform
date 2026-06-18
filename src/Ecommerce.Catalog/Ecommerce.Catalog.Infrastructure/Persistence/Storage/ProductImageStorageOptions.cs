namespace Ecommerce.Catalog.Infrastructure.Persistence.Storage;

internal sealed class ProductImageStorageOptions
{
    public const string SectionName = "ProductImageStorage";

    public string? ServiceUri { get; init; }

    //only local
    public string? ConnectionString { get; init; }

    public string ContainerName { get; init; } = "product-images";
}
