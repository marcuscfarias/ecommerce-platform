using Ecommerce.Shared.Application.Commands;

namespace Ecommerce.Catalog.Application.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string? Description) : ICommand<int>;
