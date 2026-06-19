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
        await _container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var blobName = $"{Guid.NewGuid():N}{ResolveExtension(contentType)}";
        var blob = _container.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blob.UploadAsync(content, new BlobUploadOptions { HttpHeaders = headers }, ct);

        return blob.Uri.ToString();
    }

    public Task DeleteAsync(string imageUrl, CancellationToken ct = default)
    {
        var blobName = new Uri(imageUrl).Segments[^1];
        var blob = _container.GetBlobClient(blobName);

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
