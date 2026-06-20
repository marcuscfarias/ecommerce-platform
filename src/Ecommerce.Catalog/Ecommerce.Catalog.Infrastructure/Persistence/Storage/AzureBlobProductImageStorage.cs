using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ecommerce.Catalog.Domain.Storage;
using Microsoft.Extensions.Options;

namespace Ecommerce.Catalog.Infrastructure.Persistence.Storage;

internal sealed class AzureBlobProductImageStorage(
    BlobServiceClient client,
    IOptions<ProductImageStorageOptions> options) : IProductImageStorage
{
    private readonly BlobContainerClient _container = client.GetBlobContainerClient(options.Value.ContainerName);

    public async Task<string> UploadAsync(Stream content, string contentType, CancellationToken ct = default)
    {
        await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        var blobName = $"{Guid.NewGuid():N}{ResolveExtension(contentType)}";
        var blob = _container.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blob.UploadAsync(content, new BlobUploadOptions { HttpHeaders = headers }, ct);

        return blob.Name;
    }

    public async Task<ProductImageDownload?> DownloadAsync(string imageKey, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(imageKey);

        var imageMissing = !await blob.ExistsAsync(ct);
        if (imageMissing)
            return null;

        var download = await blob.DownloadContentAsync(ct);
        return new ProductImageDownload(
            download.Value.Content.ToArray(),
            download.Value.Details.ContentType);
    }

    public Task DeleteAsync(string imageKey, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(imageKey);

        return blob.DeleteIfExistsAsync(cancellationToken: ct);
    }

    private static string ResolveExtension(string contentType) => contentType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => string.Empty,
    };
}
