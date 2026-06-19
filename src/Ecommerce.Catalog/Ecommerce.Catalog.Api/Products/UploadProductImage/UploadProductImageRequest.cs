using Ecommerce.Catalog.Application.Products.UploadProductImage;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Catalog.Api.Products.UploadProductImage;

public sealed record UploadProductImageRequest(IFormFile File)
{
    internal UploadProductImageCommand ToCommand(int id) =>
        new(id, File.OpenReadStream(), File.ContentType);
}
