using Ecommerce.Shared.Application.CQRS;

namespace Ecommerce.Catalog.Application.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(int Id, string Name, string Slug, string? Description, bool IsActive) : ICommand;
