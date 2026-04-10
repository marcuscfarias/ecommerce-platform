using Ecommerce.Shared.Application.CQRS;

namespace Ecommerce.Catalog.Application.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(int Id) : IQuery<GetCategoryByIdResult>;
