using Ecommerce.Catalog.Application.Products.CreateProduct;

namespace Ecommerce.Catalog.Api.Products.CreateProduct;

public sealed record CreateProductRequest(
    string Name,
    decimal Price,
    string Sku,
    int CategoryId,
    int StockQuantity,
    string? Description = null)
{
    internal CreateProductCommand ToCommand() =>
        new(Name, Description, Price, Sku, CategoryId, StockQuantity);
}
