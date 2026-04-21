using Ecommerce.Shared.Application.CQRS;

namespace Ecommerce.Catalog.Application.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(int Id) : ICommand;
