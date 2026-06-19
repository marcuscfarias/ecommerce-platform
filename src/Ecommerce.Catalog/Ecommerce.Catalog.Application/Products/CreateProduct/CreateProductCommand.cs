using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Catalog.Application.Products.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    string Sku,
    int CategoryId,
    int StockQuantity) : ICommand<int>;
