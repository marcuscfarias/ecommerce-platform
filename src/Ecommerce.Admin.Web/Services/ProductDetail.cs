namespace Ecommerce.Admin.Web.Services;

public sealed record ProductDetail(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    string Sku,
    int CategoryId,
    int StockQuantity,
    bool IsActive,
    string? ImageUrl);
