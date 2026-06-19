using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Products.RemoveProductImage;

public sealed record RemoveProductImageCommand(int Id) : ICommand;
