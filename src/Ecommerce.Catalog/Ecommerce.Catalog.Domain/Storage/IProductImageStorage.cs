namespace Ecommerce.Catalog.Domain.Storage;

public interface IProductImageStorage
{
    Task<string> UploadAsync(Stream content, string contentType, CancellationToken ct = default);

    Task<ProductImageDownload?> DownloadAsync(string imageKey, CancellationToken ct = default);

    Task DeleteAsync(string imageKey, CancellationToken ct = default);
}
