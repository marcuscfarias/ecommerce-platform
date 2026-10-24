namespace Ecommerce.Catalog.Application.Products.GetProductAvailability;

public sealed record GetProductAvailabilityResult(IReadOnlyList<ProductAvailability> Items);

public sealed record ProductAvailability(
    int Id,
    string Name,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    bool HasImage);
