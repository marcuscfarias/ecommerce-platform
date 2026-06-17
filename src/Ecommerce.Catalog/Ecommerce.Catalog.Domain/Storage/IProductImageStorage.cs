namespace Ecommerce.Catalog.Domain.Storage;

public interface IProductImageStorage
{
    Task<string> UploadAsync(Stream content, string contentType, CancellationToken ct = default);

    Task DeleteAsync(string imageUrl, CancellationToken ct = default);
}
