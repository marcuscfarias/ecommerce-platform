using System.Text.Json;
using Ecommerce.Orders.Application.Carts.GetMyCart;

namespace Ecommerce.Orders.Api.Carts.GetMyCart;

public sealed record GetMyCartResponse(
    IReadOnlyList<GetMyCartItemResponse> Items,
    int TotalQuantity,
    decimal Total)
{
    internal static GetMyCartResponse FromResult(GetMyCartResult result) =>
        new(
            result.Items.Select(GetMyCartItemResponse.FromResult).ToList(),
            result.TotalQuantity,
            result.Total);
}

public sealed record GetMyCartItemResponse(
    int ProductId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal,
    bool HasImage,
    string Status,
    int? AvailableQuantity)
{
    internal static GetMyCartItemResponse FromResult(GetMyCartItemResult result) =>
        new(
            result.ProductId,
            result.Name,
            result.UnitPrice,
            result.Quantity,
            result.Subtotal,
            result.HasImage,
            JsonNamingPolicy.CamelCase.ConvertName(result.Status.ToString()),
            result.AvailableQuantity);
}
