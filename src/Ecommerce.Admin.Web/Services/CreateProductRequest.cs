namespace Ecommerce.Admin.Web.Services;

public sealed record CreateProductRequest(
    string Name,
    decimal Price,
    string Sku,
    int CategoryId,
    int StockQuantity,
    string? Description);
