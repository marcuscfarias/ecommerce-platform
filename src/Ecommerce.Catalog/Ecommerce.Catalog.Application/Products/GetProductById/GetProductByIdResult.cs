namespace Ecommerce.Catalog.Application.Products.GetProductById;

public sealed record GetProductByIdResult(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    string Sku,
    int CategoryId,
    int StockQuantity,
    bool IsActive,
    string? ImageKey);
