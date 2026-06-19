using Ecommerce.Catalog.Application.Products.UpdateProduct;

namespace Ecommerce.Catalog.Api.Products.UpdateProduct;

public sealed record UpdateProductRequest(
    string Name,
    decimal Price,
    string Sku,
    int CategoryId,
    int StockQuantity,
    string? Description = null)
{
    internal UpdateProductCommand ToCommand(int id) =>
        new(id, Name, Description, Price, Sku, CategoryId, StockQuantity);
}
