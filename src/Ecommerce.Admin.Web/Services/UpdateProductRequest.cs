namespace Ecommerce.Admin.Web.Services;

public sealed record UpdateProductRequest(
    string Name,
    decimal Price,
    string Sku,
    int CategoryId,
    int StockQuantity,
    string? Description);
