using Ecommerce.Catalog.Application.Products.GetProductById;

namespace Ecommerce.Catalog.Api.Products.GetProductById;

public sealed record GetProductByIdResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    string Sku,
    int CategoryId,
    int StockQuantity,
    bool IsActive,
    string? ImageUrl)
{
    internal static GetProductByIdResponse FromResult(GetProductByIdResult result) =>
        new(
            result.Id,
            result.Name,
            result.Description,
            result.Price,
            result.Currency,
            result.Sku,
            result.CategoryId,
            result.StockQuantity,
            result.IsActive,
            result.ImageKey is null ? null : $"api/v1/products/{result.Id}/image");
}
