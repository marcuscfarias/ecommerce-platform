using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Products.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IQuery<GetProductByIdResult>;
