using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Products.UploadProductImage;

public sealed record UploadProductImageCommand(int Id, Stream Content, string ContentType) : ICommand;
