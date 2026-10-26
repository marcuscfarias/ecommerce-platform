namespace Ecommerce.Orders.Domain.Catalog;

public sealed record CatalogProduct(
    int Id,
    string Name,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    bool HasImage);
