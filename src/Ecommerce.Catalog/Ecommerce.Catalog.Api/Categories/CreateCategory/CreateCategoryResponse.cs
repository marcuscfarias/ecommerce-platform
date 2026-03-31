namespace Ecommerce.Catalog.Api.Categories.CreateCategory;

public sealed record CreateCategoryResponse(int Id, string Name, string Slug, string? Description = null);