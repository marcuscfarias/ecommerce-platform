using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Products.SetProductStatus;

public sealed record SetProductStatusCommand(int Id, bool IsActive) : ICommand;
