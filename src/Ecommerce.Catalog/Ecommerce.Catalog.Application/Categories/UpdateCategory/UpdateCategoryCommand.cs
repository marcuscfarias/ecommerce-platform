using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(int Id, string Name, string? Description) : ICommand;
