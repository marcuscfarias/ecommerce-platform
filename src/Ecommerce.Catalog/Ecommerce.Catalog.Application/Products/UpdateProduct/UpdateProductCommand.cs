using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    string Sku,
    int CategoryId,
    int StockQuantity) : ICommand;
