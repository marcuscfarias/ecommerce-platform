using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Storefront.Categories.ListStorefrontCategories;

public sealed record ListStorefrontCategoriesQuery : IQuery<ListStorefrontCategoriesResult>;
