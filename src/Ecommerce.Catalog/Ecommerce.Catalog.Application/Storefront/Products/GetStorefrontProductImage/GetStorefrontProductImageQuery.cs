using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Storefront.Products.GetStorefrontProductImage;

public sealed record GetStorefrontProductImageQuery(int Id) : IQuery<GetStorefrontProductImageResult>;
