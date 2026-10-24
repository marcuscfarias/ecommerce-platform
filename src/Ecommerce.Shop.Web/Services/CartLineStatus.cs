using System.Text.Json.Serialization;

namespace Ecommerce.Shop.Web.Services;

[JsonConverter(typeof(JsonStringEnumConverter<CartLineStatus>))]
public enum CartLineStatus
{
    Available = 1,
    OutOfStock,
    InsufficientStock,
    Unavailable,
}
