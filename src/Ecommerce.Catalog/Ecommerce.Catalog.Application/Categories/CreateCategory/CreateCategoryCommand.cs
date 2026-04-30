using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string Slug, string? Description) : ICommand<int>;
