using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Products.GetProductImage;

public sealed record GetProductImageQuery(int Id) : IQuery<GetProductImageResult>;
