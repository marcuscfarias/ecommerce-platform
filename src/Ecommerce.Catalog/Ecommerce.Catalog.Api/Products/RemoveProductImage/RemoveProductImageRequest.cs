using Ecommerce.Catalog.Application.Products.RemoveProductImage;

namespace Ecommerce.Catalog.Api.Products.RemoveProductImage;

public sealed record RemoveProductImageRequest
{
    internal static RemoveProductImageCommand ToCommand(int id) => new(id);
}
