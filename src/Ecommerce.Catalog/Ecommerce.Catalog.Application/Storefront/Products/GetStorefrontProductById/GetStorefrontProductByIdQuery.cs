using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductById;

public sealed record GetStorefrontProductByIdQuery(int Id) : IQuery<GetStorefrontProductByIdResult>;
