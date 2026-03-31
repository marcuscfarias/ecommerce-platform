namespace Ecommerce.Catalog.Api.Categories.GetCategoryById;

public sealed record GetCategoryByIdResponse(int Id, string Name, string Slug, string? Description);
