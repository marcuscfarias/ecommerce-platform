using Ecommerce.Catalog.Application.Products.SetProductStatus;

namespace Ecommerce.Catalog.Api.Products.SetProductStatus;

public sealed record SetProductStatusRequest(bool IsActive)
{
    internal SetProductStatusCommand ToCommand(int id) => new(id, IsActive);
}
