namespace Ecommerce.Catalog.Api.Categories.CreateCategory;

public sealed record CreateCategoryRequest(string Name, string Slug, string? Description);
